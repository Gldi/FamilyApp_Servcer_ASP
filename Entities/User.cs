namespace Fm.Api.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;

    public string? DisplayName { get; set; }
    public string? ProfileImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}