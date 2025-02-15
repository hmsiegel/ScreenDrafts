﻿var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddReverseProxy()
  .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services
  .AddOpenTelemetry()
  .ConfigureResource(resource => resource.AddService(DiagnosticsConfig.ServiceName))
  .WithTracing(tracing =>
  {
    tracing
      .AddAspNetCoreInstrumentation()
      .AddHttpClientInstrumentation()
      .AddSource("Yarp.ReverseProxy");

    tracing.AddOtlpExporter();
  });

builder.Services.AddAuthorization();

builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.ConfigureOptions<JwtBearerConfigureOptions>();

var app = builder.Build();

app.UseLogContextTraceLogging();

app.UseSerilogRequestLogging();

app.UseAuthentication();

app.UseAuthorization();

app.MapReverseProxy();

await app.RunAsync();

