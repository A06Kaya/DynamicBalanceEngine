using DynamicBalanceEngine.Backend.Data;
using DynamicBalanceEngine.Backend.Models;
using Microsoft.AspNetCore.Mvc;

namespace DynamicBalanceEngine.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LogsController : ControllerBase
{
    private readonly AppDbContext _context;

    public LogsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLog([FromBody] LogDto logDto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var log = new Log
        {
            UserId = logDto.UserId,
            ActionType = logDto.ActionType,
            IsError = logDto.IsError,
            Details = logDto.Details,
            Timestamp = DateTime.UtcNow
        };

        _context.Logs.Add(log);
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Log received" });
    }
}

public class LogDto
{
    public Guid UserId { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public bool IsError { get; set; }
    public string? Details { get; set; }
}
