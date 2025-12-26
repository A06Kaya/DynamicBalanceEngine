using DynamicBalanceEngine.Backend.Data;
using DynamicBalanceEngine.Backend.Models;
using DynamicBalanceEngine.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DynamicBalanceEngine.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RiskService _riskService;

    public UsersController(AppDbContext context, RiskService riskService)
    {
        _context = context;
        _riskService = riskService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
        {
            return BadRequest("Username already exists");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = dto.Username,
            Balance = dto.InitialBalance
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();
        return Ok(user);
    }

    [HttpPost("{id}/calculate-risk")]
    public async Task<IActionResult> CalculateRisk(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        try
        {
            var riskScore = await _riskService.CalculateRiskAsync(id);
            return Ok(riskScore);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}

public class CreateUserDto
{
    public string Username { get; set; } = string.Empty;
    public decimal InitialBalance { get; set; }
}
