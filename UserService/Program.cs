using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

builder.Services.AddDbContext<UserDbContext>(options =>
{
    string connectionString = environment switch
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

builder.Services.AddControllers();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<UserDbContext>();

    try
    {
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while applying migrations: {ex.Message}");
    }
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

