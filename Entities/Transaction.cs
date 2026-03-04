namespace Fm.Api.Entities
{
    public enum TxType { Income =1, Expense = 2 }

    /// <summary>
    /// 가계부 거래 내역 모델
    /// </summary>
    public class Transaction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public TxType Type { get; set; }
        public string Category { get; set; } = default!;
        public decimal Amount { get; set; }
        public string? Memo { get; set; }
        public DateTime SpentAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
