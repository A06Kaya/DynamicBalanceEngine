using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models
{
    public class BlogPost
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10)]
        public string Content { get; set; }

        public int AuthorId { get; set; }
        
        public string AuthorUsername { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
