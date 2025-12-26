using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models;

public class RiskScore
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public double Score { get; set; }

    public bool IsHighRisk { get; set; }

    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}
