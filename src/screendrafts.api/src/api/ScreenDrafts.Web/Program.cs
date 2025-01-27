var builder = WebApplication.CreateBuilder(args);

Assembly[] presentationAssemblies = [
  ScreenDrafts.Modules.Drafts.Presentation.AssemblyReference.Assembly
  ];

Assembly[] applicationAssembles = [
  ScreenDrafts.Modules.Drafts.Application.AssemblyReference.Assembly
  ];

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = presentationAssemblies;
});

builder.Services.AddApplication(applicationAssembles);

var databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

builder.Services.AddInfrastructure(
  databaseConnectionString,
  redisConnectionString);

builder.Configuration.AddModuleConfiguration(["drafts"]);

builder.Services.AddHealthChecks()
  .AddNpgSql(databaseConnectionString)
  .AddRedis(redisConnectionString);

builder.Services.AddDraftsModule(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();

  app.ApplyMigrations();
}

app.UseFastEndpoints();

app.MapHealthChecks("health", new HealthCheckOptions
{
  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

await app.RunAsync();

