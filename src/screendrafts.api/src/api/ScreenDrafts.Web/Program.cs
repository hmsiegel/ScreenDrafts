var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

var databaseConnectionString = builder.Configuration.GetConnectionString("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionString("Cache")!;

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

builder.Services.AddHealthChecks()
  .AddNpgSql(databaseConnectionString)
  .AddRedis(redisConnectionString)
  .AddUrlGroup(new Uri(builder.Configuration.GetValue<string>("KeyCloak:HealthUrl")!), HttpMethod.Get, "keycloak");

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

