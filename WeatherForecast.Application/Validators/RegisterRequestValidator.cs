using FluentValidation;
using WeatherForecast.Application.Common.Localization;
using WeatherForecast.Application.DTOs;

namespace WeatherForecast.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IAppLocalizer _localizer)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(_localizer["UsernameRequired"]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(_localizer["PasswordRequired"]);
    }
}

