namespace Fm.Api.Entities;

/// <summary>
/// 가족 간 관계선을 나타내는 엔티티
/// </summary>
public class FamilyRelationship
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // 어느 가족에 속하는 관계인지 (FamilyId로 FamilyMember의 FamilyId와 매칭)
    public Guid FamilyId { get; set; }

    // 관계의 시작점이 되는 가족 구성원 (예: 아버지) FromMemberId → ToMemberId (예: 자녀)
    public Guid FromMemberId { get; set; }

    public Guid ToMemberId { get; set; }

    // 관계 유형 (예: 부모-자녀, 형제-자매, 배우자 등)
    // 엄마 → 아이 = parent
    // 아이 → 엄마 = child
    // 엄마 → 아빠 = spouse
    public string RelationType { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}