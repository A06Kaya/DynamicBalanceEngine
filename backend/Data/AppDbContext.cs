using Microsoft.EntityFrameworkCore;
using DynamicBalanceEngine.Backend.Models;

namespace DynamicBalanceEngine.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<RiskLog> RiskLogs { get; set; }
    }
}
