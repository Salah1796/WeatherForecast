namespace WeatherForecast.Domain.Common;

/// <summary>
/// Base class for entities that need auditing.
/// </summary>
public class AuditableEntity : IAuditableEntity
{
    /// <summary>
    /// The date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// The date and time when the entity was deleted (for soft delete).
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Indicates whether the entity is marked as deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}