using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public double RiskScore { get; set; } = 0.0; // 0 to 100
    }
}
