var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.ConfigureLogging();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Register services
builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

// Apply migrations and seed data
app.SeedDatabase();

// Configure middlewares
app.ConfigureMiddlewares();

// Log that the application is ready
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Application Started");

app.Run();