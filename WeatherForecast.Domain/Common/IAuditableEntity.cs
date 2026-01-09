namespace WeatherForecast.Domain.Common;

/// <summary>
/// Defines auditing properties for entities.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// The date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the entity was last updated.
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The date and time when the entity was deleted (for soft delete).
    /// </summary>
    DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Indicates whether the entity is marked as deleted (soft delete).
    /// </summary>
    bool IsDeleted { get; set; }
}
