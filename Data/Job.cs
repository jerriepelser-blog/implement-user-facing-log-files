namespace ImplementUserFacingLogFiles.Data;

public class Job
{
    public required bool HasErrors { get; set; }

    public required bool HasWarnings { get; set; }

    public required Guid Id { get; set; }

    public required DateTime QueuedAt { get; set; }
}