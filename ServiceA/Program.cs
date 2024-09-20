using System.Diagnostics.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Logs;
using OpenTelemetry.Extensions.Hosting; // Necessary for the AddOpenTelemetry extensions

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

const string serviceName = "Dstrbtd-trace-serviceA";

// Retrieve OTLP endpoint from appsettings.json or fallback to the default endpoint
var otlpEndpoint = builder.Configuration["OTLP_ENDPOINT_URL"] ?? "http://40.71.253.33:4317";

builder.Services.AddHttpClient("backend", client =>
{
    client.BaseAddress = new Uri("http://localhost:5001");
});

// Setup OpenTelemetry Logging
builder.Logging.ClearProviders(); // Optional: Clear default logging providers
builder.Logging.AddOpenTelemetry(options =>
{
    options
        .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(otlpEndpoint); // Export logs to OTLP endpoint
        })
        .AddConsoleExporter(); // Optional: Also log to the console
});

// Setup OpenTelemetry Tracing and Metrics
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(otlpOptions =>
        {
            otlpOptions.Endpoint = new Uri(otlpEndpoint); // Send traces to OTLP endpoint
        })
        .AddConsoleExporter())
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter(otlpOptions =>
            {
                otlpOptions.Endpoint = new Uri(otlpEndpoint); // Send metrics to OTLP endpoint
            })
            .AddConsoleExporter();
    });

// Define custom Meter and Metrics
Meter meter = new Meter("MyApp.Metrics", "1.0.0");

// Create a counter to track the number of requests to the weather forecast endpoint
Counter<long> weatherForecastRequestsCounter = meter.CreateCounter<long>("weather_forecast_requests_total");

// Create a histogram to track the request duration
Histogram<double> requestDurationHistogram = meter.CreateHistogram<double>("request_duration_seconds");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
