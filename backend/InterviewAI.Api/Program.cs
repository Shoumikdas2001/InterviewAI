using InterviewAI.Api.Extensions;
using InterviewAI.Api.Middleware;
using Serilog;
using Serilog.Events;

// Bootstrap logger for startup errors
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console();
    });

    // Services
    builder.Services.AddMongoDb(builder.Configuration);
    builder.Services.AddRepositories();
    builder.Services.AddApplicationServices();
    builder.Services.AddAIProvider();
    builder.Services.AddJwtAuthentication(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddSwagger();
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = System.Threading.RateLimiting.PartitionedRateLimiter.Create<HttpContext, string>(context =>
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return System.Threading.RateLimiting.RateLimitPartition.GetFixedWindowLimiter(ipAddress, _ =>
                new System.Threading.RateLimiting.FixedWindowRateLimiterOptions
                {
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst,
                    QueueLimit = 10
                });
        });
        options.RejectionStatusCode = 429;
    });

    // File upload limit: 10MB (resume is 5MB + overhead)
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(opts =>
    {
        opts.MultipartBodyLengthLimit = 10 * 1024 * 1024;
    });

    var app = builder.Build();

    // Middleware pipeline
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "InterviewAI API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("InterviewAIPolicy");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => new { status = "healthy", timestamp = DateTime.UtcNow })
        .AllowAnonymous();

    Log.Information("InterviewAI API starting on {Environment}", app.Environment.EnvironmentName);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "InterviewAI API failed to start");
}
finally
{
    Log.CloseAndFlush();
}
