using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

builder.Services.AddControllers();

builder.Configuration.AddJsonFile("appsettings.json", true, true);
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

var jwtSecret = builder.Configuration["jwt:secret"];

if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new Exception("JWT secret is not configured properly.");
}

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
    ).AddJwtBearer(
    JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    }
);

builder.Services.AddOcelot(builder.Configuration);

builder.Services.AddHsts(options =>
{
    options.Preload = true;
    options.IncludeSubDomains = true;
    options.MaxAge = TimeSpan.FromDays(365); // Customize as needed
});

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var certPath = Environment.GetEnvironmentVariable("CERT_PATH") 
                   ?? Path.Combine(Directory.GetCurrentDirectory(), "certs", "myapp.pfx");
    var certPassword = Environment.GetEnvironmentVariable("CERT_PASSWORD") 
                        ?? "jakob"; // Replace with your default password for local development

    serverOptions.ListenAnyIP(443, listenOptions =>
    {
        if (File.Exists(certPath))
        {
            listenOptions.UseHttps(certPath, certPassword);
        }
        else
        {
            Console.WriteLine($"Certificate file not found at {certPath}");
        }
    });

    serverOptions.ConfigureHttpsDefaults(co =>
    {
        co.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
    });
});


var app = builder.Build();

app.UseHttpsRedirection();

app.UseOcelot().Wait();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();