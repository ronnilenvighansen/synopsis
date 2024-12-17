using Microsoft.EntityFrameworkCore;
using PostService.Controllers;
using PostService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDValidationService>();

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

builder.Services.AddDbContext<PostDbContext>(options =>
{
    var connectionString = environment switch
    {
        "Development" => builder.Configuration.GetConnectionString("DevelopmentConnection"),
        "Docker" => builder.Configuration.GetConnectionString("DockerConnection"),
        _ => throw new Exception("No valid environment detected")
    };

    options.UseMySql(
        connectionString,
        new MySqlServerVersion(new Version(8, 0, 0)),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure()
    );
});

builder.Services.AddHttpClient<IDValidationService>((provider, client) =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var baseAddress = environment switch
    {
        "Development" => configuration["BaseAddresses:DevelopmentBaseAddress"],
        "Docker" => configuration["BaseAddresses:DockerBaseAddress"],
        _ => throw new Exception("No valid environment detected")
    };

    Console.WriteLine($"Configuring HttpClient in AddHttpClient with BaseAddress: {baseAddress}");
    client.BaseAddress = new Uri(baseAddress);
});

builder.Services.AddScoped<PostDbContext>();

builder.Services.AddHttpClient<PostController>();

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PostDbContext>();
    dbContext.Database.Migrate();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
