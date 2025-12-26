using Microsoft.AspNetCore.Mvc;
using DynamicBalanceEngine.Backend.Data;
using System;
using System.Threading.Tasks;

namespace DynamicBalanceEngine.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RiskController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RiskController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRiskStatus(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(new
            {
                UserId = user.Id,
                RiskScore = user.CurrentRiskScore,
                Level = user.SecurityLevel,
                Quarantined = user.IsQuarantined
            });
        }
    }
}
