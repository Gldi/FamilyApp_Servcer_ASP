namespace Fm.Api.Entities
{
    /// <summary>
    /// 가족 그룹을 나타내는 엔티티
    /// </summary>
    public class Family
    {
        // 가족 계정
        public Guid Id { get; set; } = Guid.NewGuid();

        // 가족 이름 ( 예: "김씨 가족", "Smith Family" 등)
        public string Name { get; set; } = default!;
        
        // 가족 그룹을 만든 계정
        public Guid CreatedByUserId { get; set; }
        
        // 생성일시
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
