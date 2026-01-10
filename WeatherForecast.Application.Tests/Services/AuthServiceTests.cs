using FluentValidation;
using Microsoft.Extensions.Localization;
using Moq;
using WeatherForecast.Application.Common.Enums;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.DTOs;
using WeatherForecast.Application.Interfaces;
using WeatherForecast.Application.Services;
using WeatherForecast.Domain.Entities;
using WeatherForecast.Domain.Repositories;
using Xunit;

namespace WeatherForecast.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<ITokenGenerator> _tokenGeneratorMock;
    private readonly Mock<IValidator<RegisterRequest>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginRequest>> _loginValidatorMock;
    private readonly Mock<IAppLocalizer> _localizerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _tokenGeneratorMock = new Mock<ITokenGenerator>();
        _registerValidatorMock = new Mock<IValidator<RegisterRequest>>();
        _loginValidatorMock = new Mock<IValidator<LoginRequest>>();
        _localizerMock = new Mock<IAppLocalizer>();

        // Setup localizer to return the key as the value
        _localizerMock.Setup(x => x[It.IsAny<string>()])
            .Returns((string key) => new LocalizedString(key, key));

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _tokenGeneratorMock.Object,
            _registerValidatorMock.Object,
            _loginValidatorMock.Object,
            _localizerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidRequest_ReturnsSuccessResult()
    {
        // Arrange
        var request = new RegisterRequest { Username = "testuser", Password = "Password123!" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);
        const string token = "jwt_token";

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.UserExistsAsync(request.Username))
            .ReturnsAsync(false);
        _passwordHasherMock.Setup(x => x.HashPassword(request.Password))
            .Returns(hashedPassword);
        _userRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(user);
        _tokenGeneratorMock.Setup(x => x.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.Equal(token, result.Data.Token);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.Equal("UserRegisteredSuccessfully", result.Message);
        _userRepositoryMock.Verify(x => x.UserExistsAsync(request.Username), Times.Once);
        _registerValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.Is<User>(x=> x.Username == request.Username && x.PasswordHash == hashedPassword))
        , Times.Once);
        _passwordHasherMock.Verify(x => x.HashPassword(request.Password), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.Is<User>(x => x.Username == request.Username && x.PasswordHash == hashedPassword)), Times.Once);
        _localizerMock.Verify(x => x["UserRegisteredSuccessfully"], Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsConflictError()
    {
        // Arrange
        var request = new RegisterRequest { Username = "existinguser", Password = "Password123!" };
        var validationResult = new FluentValidation.Results.ValidationResult();

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.UserExistsAsync(request.Username))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Conflict, result.StatusCode);
        Assert.Equal("UsernameAlreadyExists", result.Message);
        _userRepositoryMock.Verify(x => x.UserExistsAsync(request.Username), Times.Once);
        _registerValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        _localizerMock.Verify(x => x["UsernameAlreadyExists"], Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithInvalidUsername_ReturnsValidationError()
    {
        // Arrange
        var request = new RegisterRequest { Username = "", Password = "password" };
        var validationFailure = new FluentValidation.Results.ValidationFailure("Username", "Username is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        _registerValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ValidationFailed", result.Message);
        _userRepositoryMock.Verify(x => x.UserExistsAsync(request.Username), Times.Never);
        _registerValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _userRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
        _passwordHasherMock.Verify(x => x.HashPassword(It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        _localizerMock.Verify(x => x["ValidationFailed"], Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccessResult()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "Password123!" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);
        const string token = "jwt_token";

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _tokenGeneratorMock.Setup(x => x.GenerateToken(user))
            .Returns(token);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal(user.Username, result.Data.Username);
        Assert.Equal(token, result.Data.Token);
        Assert.Equal(StatusCode.OK, result.StatusCode);
        Assert.Equal("LoginSuccessful", result.Message);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _loginValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(request.Password, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Once);
        _localizerMock.Verify(x => x["LoginSuccessful"], Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidRequest_ReturnsValidationError()
    {
        // Arrange
        var request = new LoginRequest { Username = "", Password = "" };
        var validationFailure = new FluentValidation.Results.ValidationFailure("Username", "Username is required");
        var validationResult = new FluentValidation.Results.ValidationResult([validationFailure]);

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.BadRequest, result.StatusCode);
        Assert.Equal("ValidationFailed", result.Message);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Never);
        _loginValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        _localizerMock.Verify(x => x["ValidationFailed"], Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsUnauthorizedError()
    {
        // Arrange
        var request = new LoginRequest { Username = "testuser", Password = "WrongPassword" };
        var hashedPassword = "hashed_password";
        var validationResult = new FluentValidation.Results.ValidationResult();
        var user = new User(request.Username, hashedPassword);

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync(user);
        _passwordHasherMock.Setup(x => x.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("InvalidCredentials", result.Message);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _loginValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(request.Password, user.PasswordHash), Times.Once);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(user), Times.Never);
        _localizerMock.Verify(x => x["InvalidCredentials"], Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsUnauthorizedError()
    {
        // Arrange
        var request = new LoginRequest { Username = "nonexistentuser", Password = "Password123!" };
        var validationResult = new FluentValidation.Results.ValidationResult();

        _loginValidatorMock.Setup(x => x.ValidateAsync(request, CancellationToken.None))
            .ReturnsAsync(validationResult);
        _userRepositoryMock.Setup(x => x.GetByUsernameAsync(request.Username))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(StatusCode.Unauthorized, result.StatusCode);
        Assert.Equal("InvalidCredentials", result.Message);
        _userRepositoryMock.Verify(x => x.GetByUsernameAsync(request.Username), Times.Once);
        _loginValidatorMock.Verify(x => x.ValidateAsync(request, CancellationToken.None), Times.Once);
        _passwordHasherMock.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _tokenGeneratorMock.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
        _localizerMock.Verify(x => x["InvalidCredentials"], Times.Once);
    }
}
