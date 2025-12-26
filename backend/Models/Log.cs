using System;

namespace DynamicBalanceEngine.Backend.Models
{
    public class RiskLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string ActionType { get; set; } = string.Empty; // Login, API_Call
        public string Result { get; set; } = string.Empty; // Success, Failed
        public double OldRiskScore { get; set; }
        public double NewRiskScore { get; set; }
        public double RiskDelta { get; set; }
        
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Details { get; set; } = string.Empty; // For threat info
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
