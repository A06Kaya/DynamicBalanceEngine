using System.Diagnostics;
using System.Text.Json;
using DynamicBalanceEngine.Backend.Models;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;

namespace DynamicBalanceEngine.Backend.Services
{
    public class RiskService
    {
        private readonly ILogger<RiskService> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly Data.AppDbContext _context;

        public RiskService(ILogger<RiskService> logger, IWebHostEnvironment env, Data.AppDbContext context)
        {
            _logger = logger;
            _env = env;
            _context = context;
        }

        public async Task ApplyTimeDecayAsync(User user)
        {
            // Find the last risk update time
            var lastLog = await _context.RiskLogs
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();

            DateTime lastUpdate = lastLog != null ? lastLog.Timestamp : DateTime.UtcNow;
            
            // Calculate minutes passed
            double minutesPassed = (DateTime.UtcNow - lastUpdate).TotalMinutes;

            if (minutesPassed >= 1)
            {
                int decayAmount = (int)minutesPassed; // 1 point per minute
                if (decayAmount > 0 && user.RiskScore > 0)
                {
                    double oldScore = user.RiskScore;
                    user.RiskScore = Math.Max(0, user.RiskScore - decayAmount);

                    if (user.RiskScore != oldScore)
                    {
                        var decayLog = new RiskLog
                        {
                            UserId = user.Id,
                            ActionType = "time_decay",
                            Result = "success",
                            OldRiskScore = oldScore,
                            NewRiskScore = user.RiskScore,
                            RiskDelta = -decayAmount,
                            Details = $"Decay: {decayAmount} pts for {decayAmount} mins",
                            Timestamp = DateTime.UtcNow
                        };
                        
                        _context.RiskLogs.Add(decayLog);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        public async Task<RiskLog> EvaluateRiskAsync(User user, string action, string status, int frequency = 0, string content = "", string ipAddress = "", string userAgent = "")
        {
            var scriptPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "python_scripts", "calculate_risk.py");
            
            // Adjust for Docker environment if needed
            if (!File.Exists(scriptPath))
            {
                scriptPath = "/app/python_scripts/calculate_risk.py";
            }

            var payload = new
            {
                current_score = user.RiskScore,
                action = action,
                status = status,
                frequency = frequency,
                content = content,
                timestamp = DateTime.UtcNow
            };

            var jsonPayload = JsonSerializer.Serialize(payload);
            
            var processInfo = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = $"\"{scriptPath}\" \"{jsonPayload.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                 processInfo.FileName = "python";
            }
            
            // In Docker it's python3

            try
            {
                using var process = Process.Start(processInfo);
                using var reader = process.StandardOutput;
                using var stderr = process.StandardError;

                var result = await reader.ReadToEndAsync();
                var error = await stderr.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError($"Python Error: {error}");
                }

                var riskData = JsonSerializer.Deserialize<JsonElement>(result);
                var newScore = riskData.GetProperty("new_score").GetDouble();
                var delta = riskData.GetProperty("risk_delta").GetDouble();
                
                string details = "";
                if (riskData.TryGetProperty("issues", out var issuesElement) && issuesElement.ValueKind == JsonValueKind.Array)
                {
                    var issues = issuesElement.EnumerateArray().Select(x => x.GetString()).ToArray();
                    if (issues.Length > 0) details = string.Join(", ", issues);
                }

                if (!string.IsNullOrEmpty(ipAddress)) details += $" | IP: {ipAddress}";
                if (!string.IsNullOrEmpty(userAgent)) details += $" | UA: {userAgent}";

                var log = new RiskLog
                {
                    UserId = user.Id,
                    ActionType = action,
                    Result = status,
                    OldRiskScore = user.RiskScore,
                    NewRiskScore = newScore,
                    RiskDelta = delta,
                    Details = details.TrimStart(' ', '|'),
                    Timestamp = DateTime.UtcNow
                };

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to execute risk script");
                // Fallback or rethrow
                throw;
            }
        }
    }
}
