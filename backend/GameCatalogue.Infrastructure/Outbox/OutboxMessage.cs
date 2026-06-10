namespace GameCatalogue.Infrastructure.Outbox;

/// <summary>
/// Persistent record of a domain event awaiting dispatch (transactional outbox).
/// </summary>
public class OutboxMessage
{
    /// <summary>Gets or sets the unique identifier of the message.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the assembly-qualified type name of the event.</summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>Gets or sets the JSON-serialized event payload.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the UTC timestamp at which the event occurred.</summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp at which the event was processed (null = unprocessed).</summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Gets or sets the last error encountered while processing (null = none).</summary>
    public string? Error { get; set; }
}
