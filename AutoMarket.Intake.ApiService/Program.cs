using AutoMarket.Intake.ApiService;
using AutoMarket.Intake.ApiService.Data; // <--- NEW IMPORT
using AutoMarket.Intake.ApiService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION ---
builder.AddServiceDefaults();

// --- 2. DATABASE SETUP (NEW) ---
// "postgres" matches the name you gave the container in AppHost.cs
builder.AddNpgsqlDbContext<IntakeDbContext>("postgres");

// --- 3. TELEMETRY ---
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
{
    tracing.AddSource(Telemetry.ServiceName);
});

// --- 4. SERVICES ---
builder.Services.AddScoped<VehicleGrader>();

// --- 5. CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// --- 6. DATA INITIALIZATION (THE "SENIOR" WAY) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var db = services.GetRequiredService<IntakeDbContext>();

    // Retry loop for "Cold Start" databases
    var retries = 5;
    while (retries > 0)
    {
        try
        {
            logger.LogInformation("Attempting to migrate database...");
            db.Database.Migrate(); // The real command
            logger.LogInformation("Database migration successful.");
            break; // Success! Exit the loop.
        }
        catch (Npgsql.NpgsqlException ex)
        {
            retries--;
            if (retries == 0) throw; // If we failed 5 times, actually crash.

            logger.LogWarning(ex, "Database not ready yet. Retrying in 2 seconds...");
            System.Threading.Thread.Sleep(2000); // Wait 2 seconds
        }
    }
}

// --- 7. MIDDLEWARE ---
app.MapDefaultEndpoints();
app.UseCors("AllowFrontend");
app.UseHttpsRedirection();

// --- 8. ENDPOINTS ---

// GET Endpoint: Verify the data is actually saving
app.MapGet("/api/intake", (IntakeDbContext db) =>
{
    // Return the last 10 scans, newest first
    return db.Scans.OrderByDescending(x => x.ScannedAt).Take(10).ToList();
});

// POST Endpoint: Save the scan
app.MapPost("/api/intake", async (
    [FromBody] string vin,
    [FromServices] VehicleGrader grader,
    [FromServices] IntakeDbContext db) => // <--- Inject DB
{
    using var activity = Telemetry.ActivitySource.StartActivity("CalculateVehicleGrade");
    activity?.SetTag("vehicle.vin", vin);

    var sw = System.Diagnostics.Stopwatch.StartNew();

    // 1. Run Logic
    int fakeMileage = Random.Shared.Next(10000, 150000);
    var result = grader.GradeVehicle(vin, fakeMileage);

    sw.Stop();

    // 2. Map to Entity
    var entity = new VehicleScan
    {
        Vin = vin,
        Grade = result.Grade,
        EstimatedValue = result.EstimatedValue,
        Notes = string.Join(", ", result.Notes),
        ProcessingLatencyMs = sw.Elapsed.TotalMilliseconds
    };

    // 3. Save to Postgres
    db.Scans.Add(entity);
    await db.SaveChangesAsync(); // <--- The moment of truth

    return Results.Ok(result);
});

app.Run();