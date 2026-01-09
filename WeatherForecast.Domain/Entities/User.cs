using System.ComponentModel.DataAnnotations;
using WeatherForecast.Domain.Common;

namespace WeatherForecast.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User : AuditableEntity
    {
        /// <summary>
        /// Creates a new user with the specified username and password hash.
        /// </summary>
        /// <param name="username">The username of the user. Cannot be null or empty.</param>
        /// <param name="passwordHash">The hashed password of the user. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when username or passwordHash is null or empty.</exception>
        public User(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentNullException(nameof(passwordHash), "Password hash cannot be empty.");

            Username = username;
            PasswordHash = passwordHash;
        }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The hashed password of the user.
        /// </summary>
        public string PasswordHash { get; private set; }

    }
}
