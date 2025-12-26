using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models;

public class Log
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    [Required]
    public string ActionType { get; set; } = string.Empty;

    public bool IsError { get; set; }

    public string? Details { get; set; }

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
