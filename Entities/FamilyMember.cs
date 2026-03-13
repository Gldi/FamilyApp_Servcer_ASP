namespace Fm.Api.Entities;

/// <summary>
/// 가족 구성원을 나타내는 엔티티
/// </summary>
public class FamilyMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // 어느 가족에 속하는지
    public Guid FamilyId { get; set; }

    // 실제 앱 계정과 연결될 수도 있고 없을 수도 있음
    public Guid? UserId { get; set; }

    // 가족 구성원 이름
    public string Name { get; set; } = default!;

    // 가족 구성원과의 관계 (예: 아버지, 어머니, 형제, 자매 등)
    public string? Role { get; set; }

    // 가족 구성원의 생년월일 (선택 사항)
    public DateOnly? BirthDate { get; set; }

    // 가족 구성원의 성별 (선택 사항)
    public string? Gender { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}