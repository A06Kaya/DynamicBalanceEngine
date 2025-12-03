using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Username { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public bool IsHighRisk { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
