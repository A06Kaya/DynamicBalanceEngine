using DynamicBalanceEngine.Backend.Data;
using System.IdentityModel.Tokens.Jwt;
using DynamicBalanceEngine.Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "DynamicBalanceEngine API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token directly below (no 'Bearer ' prefix needed).",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new List<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<RiskService>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = AuthConstants.Issuer,
            ValidAudience = AuthConstants.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthConstants.SecretKey))
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

// Middleware for Dynamic Balance (Simulated inline)
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true)
    {
        // In a real scenario, we might query the DB for the latest score
        // For now, we trust the claim if not expired, or we could check DB
        // Let's implement a simple check
        var scoreClaim = context.User.FindFirst("RiskScore");
        if (scoreClaim != null && double.TryParse(scoreClaim.Value, out double riskScore))
        {
            if (riskScore > 80)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Account suspended due to high Risk Score.");
                return;
            }
        }
    }
    await next();
});

app.MapControllers();

// Apply migrations automatically (for dev)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try 
    {
        var db = services.GetRequiredService<AppDbContext>();
        // Simple retry mechanism for DB readiness
        int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                db.Database.Migrate();
                Console.WriteLine("Database migrations applied successfully.");
                break;
            }
            catch (Exception ex)
            {
                if (i == maxRetries - 1) throw; // Throw on last attempt
                Console.WriteLine($"Database migration failed (attempt {i+1}/{maxRetries}). Retrying in 2s... Error: {ex.Message}");
                System.Threading.Thread.Sleep(2000);
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while creating the database: {ex.Message}");
        // We generally want to stop here if DB is critical
    }
}

app.Run();
