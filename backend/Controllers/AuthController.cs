using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DynamicBalanceEngine.Backend.Data;
using DynamicBalanceEngine.Backend.DTOs;
using DynamicBalanceEngine.Backend.Models;
using DynamicBalanceEngine.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace DynamicBalanceEngine.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly RiskService _riskService;

        public AuthController(AppDbContext context, IConfiguration configuration, RiskService riskService)
        {
            _context = context;
            _configuration = configuration;
            _riskService = riskService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return BadRequest("Username already exists.");
            }

            // In production, use a proper password hasher like BCrypt
            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                RiskScore = 20 // Initial Risk Score as per Policy
            };

            // Hash password (IMPLEMENT HASHING IN PRODUCTION)
            user.PasswordHash = request.Password; 

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            
            if (user == null)
            {
                // Can't penalize a non-existent user, or maybe we track IP?
                return BadRequest(new { message = "User not found." });
            }

            // Apply Time Decay before login attempt
            await _riskService.ApplyTimeDecayAsync(user);

            string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            string ua = Request.Headers["User-Agent"].ToString();

            if (user.PasswordHash != request.Password)
            {
                // Failed login
                var log = await _riskService.EvaluateRiskAsync(user, "login", "failure", 0, "", ip, ua);
                _context.RiskLogs.Add(log);
                user.RiskScore = log.NewRiskScore;
                await _context.SaveChangesAsync();
                
                return BadRequest(new { message = $"Wrong password. Risk Score Increased to {user.RiskScore}" });
            }

            // Success login
            var successLog = await _riskService.EvaluateRiskAsync(user, "login", "success", 0, "", ip, ua);
            _context.RiskLogs.Add(successLog);
            user.RiskScore = successLog.NewRiskScore;
            await _context.SaveChangesAsync();

            string token = CreateToken(user);

            return Ok(new { Token = token, RiskScore = user.RiskScore });
        }

        [Microsoft.AspNetCore.Authorization.Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return Unauthorized();

            if (!int.TryParse(userIdClaim.Value, out int userId)) return BadRequest("Invalid user ID");

            var user = await _context.Users.FindAsync(userId);
            
            // Apply Time Decay
            await _riskService.ApplyTimeDecayAsync(user);

            if (user == null) return NotFound("User not found");

            // Apply Time Decay on Profile Load
            await _riskService.ApplyTimeDecayAsync(user);

            return Ok(new 
            { 
                user.Id, 
                user.Username, 
                user.Email, 
                user.RiskScore 
            });
        }

        private string CreateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthConstants.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("RiskScore", user.RiskScore.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: AuthConstants.Issuer,
                audience: AuthConstants.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
