using DynamicBalanceEngine.Backend.Data;
using DynamicBalanceEngine.Backend.Models;
using DynamicBalanceEngine.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DynamicBalanceEngine.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RiskService _riskService;

        public BlogController(AppDbContext context, RiskService riskService)
        {
            _context = context;
            _riskService = riskService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPosts()
        {
            var posts = await _context.BlogPosts
                .OrderByDescending(p => p.CreatedAt)
                .Take(20)
                .ToListAsync();
            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] BlogPostRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Apply Time Decay
            await _riskService.ApplyTimeDecayAsync(user);

            // --- TIER CHECK ---
            // 61-90: Read-Only (Block Post/Comment)
            // 91-100: Critical (Suspended - ideally block at login, but fail-safe here)
            if (user.RiskScore > 60)
            {
                return StatusCode(403, new 
                { 
                    Message = "Restricted Mode: Your Risk Score is too high (> 60). You are in Read-Only mode.",
                    CurrentRiskScore = user.RiskScore
                });
            }

            // --- ANOMALY CHECK (IP/UA Session Hijacking) ---
            string currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string currentUa = Request.Headers["User-Agent"].ToString();

            var lastLogin = await _context.RiskLogs
                .Where(r => r.UserId == userId && r.ActionType == "login" && r.Result == "success")
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();

            if (lastLogin != null && !string.IsNullOrEmpty(lastLogin.Details))
            {
                // Simple check: if last login IP is known and different from current
                if (lastLogin.Details.Contains("IP:") && !lastLogin.Details.Contains(currentIp))
                {
                    // SESSION HIJACKING SUSPECTED
                    user.RiskScore = Math.Min(100, user.RiskScore + 20); // Penalty
                    // Optionally return 401 to force re-login
                }
            }

            // --- FREQUENCY CHECK ---
            // Count actions in last 1 minute (Stricter Burst Check)
            var oneMinAgo = DateTime.UtcNow.AddMinutes(-1);
            var frequency = await _context.RiskLogs
                .CountAsync(r => r.UserId == userId && 
                                 (r.ActionType == "create_post" || r.ActionType == "create_comment") && 
                                 r.Timestamp >= oneMinAgo);

            // 2. Risk Evaluation (BEFORE SAVING CONTENT)
            var log = await _riskService.EvaluateRiskAsync(
                user, 
                "create_post", 
                "success", 
                frequency, 
                request.Content,
                currentIp,
                currentUa
            );

            // 3. Check for Blocking Threats
            if (!string.IsNullOrEmpty(log.Details) && log.Details.Contains("Threat"))
            {
                // BLOCK CONTENT
                // But still save the risk log and update user score
                user.RiskScore = log.NewRiskScore;
                _context.RiskLogs.Add(log);
                await _context.SaveChangesAsync();
                
                return BadRequest(new { Message = "Security Threat Detected. Post blocked.", NewRiskScore = user.RiskScore });
            }

            // 4. Perform Action (Safe)
            var post = new BlogPost
            {
                Title = request.Title,
                Content = request.Content,
                AuthorId = user.Id,
                AuthorUsername = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogPosts.Add(post);
            
            user.RiskScore = log.NewRiskScore;
            _context.RiskLogs.Add(log);
            await _context.SaveChangesAsync();

            // --- RESPONSE (TIER 2 WARNING) ---
            string message = "Post shared successfully.";
            if (user.RiskScore > 20 && user.RiskScore <= 60)
            {
                message = "Post shared. Warning: Your risk score is elevated. Future posts may require approval.";
            }

            return Ok(new { PostId = post.Id, NewRiskScore = user.RiskScore, Message = message });
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var comments = await _context.Comments
                .Where(c => c.BlogPostId == id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return Ok(comments);
        }

        [Authorize]
        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(int id, [FromBody] CommentRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            // Apply Time Decay
            await _riskService.ApplyTimeDecayAsync(user);

            // --- TIER CHECK ---
            if (user.RiskScore > 60)
            {
                return StatusCode(403, new 
                { 
                    Message = "Restricted Mode: Your Risk Score is too high (> 60). Commenting is disabled.",
                    CurrentRiskScore = user.RiskScore
                });
            }

            // --- ANOMALY CHECK ---
            string currentIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string currentUa = Request.Headers["User-Agent"].ToString();

            var lastLogin = await _context.RiskLogs
                .Where(r => r.UserId == userId && r.ActionType == "login" && r.Result == "success")
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
            
            if (lastLogin != null && !string.IsNullOrEmpty(lastLogin.Details))
            {
                if (lastLogin.Details.Contains("IP:") && !lastLogin.Details.Contains(currentIp))
                {
                     user.RiskScore = Math.Min(100, user.RiskScore + 20);
                }
            }

            // --- FREQUENCY CHECK ---
            var oneMinAgo = DateTime.UtcNow.AddMinutes(-1);
            var frequency = await _context.RiskLogs
                .CountAsync(r => r.UserId == userId && 
                                 (r.ActionType == "create_post" || r.ActionType == "create_comment") && 
                                 r.Timestamp >= oneMinAgo);

            // Risk Evaluation (BEFORE SAVING COMMENT)
            var log = await _riskService.EvaluateRiskAsync(
                user, 
                "create_comment", 
                "success",
                frequency,
                request.Content,
                currentIp,
                currentUa
            );

            // Check for Blocking Threats
            if (!string.IsNullOrEmpty(log.Details) && log.Details.Contains("Threat"))
            {
                user.RiskScore = log.NewRiskScore;
                _context.RiskLogs.Add(log);
                await _context.SaveChangesAsync();
                
                return BadRequest(new { Message = "Security Threat Detected. Comment blocked.", NewRiskScore = user.RiskScore });
            }

            var comment = new Comment
            {
                BlogPostId = id,
                Content = request.Content,
                AuthorId = user.Id,
                AuthorUsername = user.Username,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            
            user.RiskScore = log.NewRiskScore;
            _context.RiskLogs.Add(log);
            await _context.SaveChangesAsync();
            
            string message = "Comment added.";
            if (user.RiskScore > 20 && user.RiskScore <= 60)
            {
                message = "Comment added. Warning: Elevated risk score.";
            }

            return Ok(new { CommentId = comment.Id, NewRiskScore = user.RiskScore, Message = message });
        }
    }

    public class BlogPostRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
    }

    public class CommentRequest
    {
        public string Content { get; set; }
    }
}
