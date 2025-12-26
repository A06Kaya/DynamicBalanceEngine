using System;
using System.ComponentModel.DataAnnotations;

namespace DynamicBalanceEngine.Backend.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public int BlogPostId { get; set; }

        [Required]
        [StringLength(500, MinimumLength = 1)]
        public string Content { get; set; }

        public int AuthorId { get; set; }
        public string AuthorUsername { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
