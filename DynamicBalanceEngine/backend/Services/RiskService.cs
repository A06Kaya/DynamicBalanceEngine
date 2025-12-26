using System.Diagnostics;
using System.Text.Json;
using DynamicBalanceEngine.Backend.Data;
using DynamicBalanceEngine.Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace DynamicBalanceEngine.Backend.Services;

public class RiskService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public RiskService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<RiskScore> CalculateRiskAsync(Guid userId)
    {
        var logs = await _context.Logs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.Timestamp)
            .Take(100) // Analyze last 100 logs
            .ToListAsync();

        var riskInput = logs.Select(l => new { is_error = l.IsError }).ToList();
        var jsonInput = JsonSerializer.Serialize(riskInput);

        var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "scripting", "risk_engine.py");
        var pythonPath = "python"; // Assume python is in PATH

        var startInfo = new ProcessStartInfo
        {
            FileName = pythonPath,
            Arguments = scriptPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        await process.StandardInput.WriteAsync(jsonInput);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Risk engine failed: {error}");
        }

        var result = JsonSerializer.Deserialize<RiskResult>(output, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        if (result == null) throw new Exception("Failed to parse risk result");

        var riskScore = new RiskScore
        {
            UserId = userId,
            Score = result.RiskScore,
            IsHighRisk = result.IsHighRisk,
            CalculatedAt = DateTime.UtcNow
        };

        _context.RiskScores.Add(riskScore);
        
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.IsHighRisk = result.IsHighRisk;
        }

        await _context.SaveChangesAsync();

        return riskScore;
    }

    private class RiskResult
    {
        public double RiskScore { get; set; }
        public bool IsHighRisk { get; set; }
    }
}
