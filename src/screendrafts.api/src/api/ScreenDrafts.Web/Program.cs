var builder = WebApplication.CreateBuilder(args);

Assembly[] presentationAssemblies = [
  ScreenDrafts.Modules.Administration.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Audit.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Communications.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Drafts.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Integrations.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Movies.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.RealTimeUpdates.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Reporting.Presentation.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Users.Presentation.AssemblyReference.Assembly,
  ];

Assembly[] applicationAssembles = [
  ScreenDrafts.Modules.Administration.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Audit.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Communications.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Drafts.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Integrations.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Movies.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.RealTimeUpdates.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Reporting.Application.AssemblyReference.Assembly,
  ScreenDrafts.Modules.Users.Application.AssemblyReference.Assembly
  ];

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = presentationAssemblies;
});

var databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

builder.Services.AddApplication(applicationAssembles);
builder.Services.AddInfrastructure(databaseConnectionString, redisConnectionString);

builder.Configuration.AddModuleConfiguration([
  "administration",
  "audit",
  "communications",
  "drafts",
  "integrations",
  "movies",
  "realtimeupdates",
  "reporting",
  "users"
  ]);

builder.Services.AddHealthChecks()
  .AddNpgSql(databaseConnectionString)
  .AddRedis(redisConnectionString);

builder.Services.AddAdministrationModule(builder.Configuration);
builder.Services.AddAuditModule(builder.Configuration);
builder.Services.AddCommunicationsModule(builder.Configuration);
builder.Services.AddDraftsModule(builder.Configuration);
builder.Services.AddIntegrationsModule(builder.Configuration);
builder.Services.AddMoviesModule(builder.Configuration);
builder.Services.AddRealTimeUpdatesModule(builder.Configuration);
builder.Services.AddReportingModule(builder.Configuration);
builder.Services.AddUsersModule(builder.Configuration);

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

