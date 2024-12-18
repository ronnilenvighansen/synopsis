var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.AggregatorService.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();