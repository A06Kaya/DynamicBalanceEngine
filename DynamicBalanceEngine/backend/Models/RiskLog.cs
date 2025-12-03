using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models
{
    public class RiskLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public string ActionType { get; set; } // Login, FileUpload, APIRequest

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public bool IsError { get; set; }

        public string Details { get; set; }
    }
}
