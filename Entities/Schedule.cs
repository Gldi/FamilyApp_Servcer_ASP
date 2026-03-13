namespace Fm.Api.Entities;

public class Schedule
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FamilyId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public bool IsAllDay { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}