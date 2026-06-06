using Hangfire;
using Opti_Sec_Backend;
using Opti_Sec_Backend.Entities;
using Opti_Sec_Backend.Hubs;
using Opti_Sec_Backend.Persistence;
using Serilog;
using Opti_Sec_Backend.Services.SessionServices;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDependencies(builder.Configuration);

// CORS for mobile app and device connections
builder.Services.AddCors(options =>
{
    options.AddPolicy("OptiSecPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Host.UseSerilog((context, configuration) =>
              configuration.ReadFrom.Configuration(context.Configuration)
            );


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHangfireDashboard("/jobs");

// Schedule recurring job: expire stale gate sessions every minute
RecurringJob.AddOrUpdate<SessionCleanupJob>(
    "expire-stale-sessions",
    job => job.ExpireStaleSessionsAsync(),
    Cron.Minutely);

app.UseSerilogRequestLogging();


app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.UseStaticFiles();

app.MapControllers();
app.MapHub<GateHub>("/hubs/gate");

app.UseExceptionHandler();


app.Run();
