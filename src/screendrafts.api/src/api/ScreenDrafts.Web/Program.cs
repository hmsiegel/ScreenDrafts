var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache")!;

builder.Services.AddApplication(AssemblyReferences.ApplicationAssemblies);
builder.Services.AddInfrastructure(
  [DraftsModule.ConfigureConsumers],
  databaseConnectionString,
  redisConnectionString);

builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = AssemblyReferences.PresentationAssemblies;
});

builder.Configuration.AddModuleConfiguration(ModuleReferences.Modules);

var keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder.Services.AddHealthChecks()
  .AddNpgSql(databaseConnectionString)
  .AddRedis(redisConnectionString)
  .AddKeyCloak(keyCloakHealthUrl);

ModuleServiceExtensions.AddModules(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
  app.MapScalarApiReference();

  app.ApplyMigrations();
}


app.MapHealthChecks("health", new HealthCheckOptions
{
  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

await app.RunAsync();

