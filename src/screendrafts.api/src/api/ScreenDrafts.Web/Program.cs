var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));

var vaultSection = builder.Configuration.GetSection("Vault");

if (!builder.Environment.IsEnvironment("Testing"))
{
  builder.Configuration.AddVaultConfiguration(
    () =>
      new VaultOptions(vaultAddress: vaultSection["Address"]!, vaultToken: vaultSection["Token"]!),
    "screendrafts",
    vaultSection["MountPoint"]
  );
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var configuration = builder.Configuration;

var databaseConnectionString = builder.Services.AddPostgresDatabase(configuration);
var redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache")!;
var rabbitMqSettings = new RabbitMqSettings(
  builder.Configuration.GetConnectionStringOrThrow("Queue")
);
var mongoConnectionString = builder.Configuration.GetConnectionStringOrThrow("Mongo")!;

builder.Services.AddApplication(AssemblyReferences.FeatureAssemblies, configuration);

builder.Services.AddInfrastructure(
  configuration,
  DiagnosticsConfig.ServiceName,
  [
    AdministrationModule.ConfigureConsumers,
    AuditModule.ConfigureConsumers,
    CommunicationsModule.ConfigureConsumers,
    DraftsModule.ConfigureConsumers,
    IntegrationsModule.ConfigureConsumers,
    MoviesModule.ConfigureConsumers,
    ReportingModule.ConfigureConsumers,
    RealTimeUpdatesModule.ConfigureConsumers,
    UsersModule.ConfigureConsumers,
  ],
  [AuditModule.ConfigureEndpoints],
  rabbitMqSettings,
  redisConnectionString,
  mongoConnectionString,
  databaseConnectionString,
  AssemblyReferences.InfrastructureAssemblies
);
builder.Services.AddPresentation();

builder
  .Services.AddFastEndpoints(opt =>
  {
    opt.Assemblies = AssemblyReferences.FeatureAssemblies;
  })
  .SwaggerDocument(o => o.ShortSchemaNames = true);

builder.Services.ConfigureOpenApi(builder.Configuration);

builder.Configuration.AddModuleConfiguration(ModuleReferences.Modules);

var keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder
  .Services.AddSingleton(sp =>
  {
    var factory = new ConnectionFactory { Uri = new Uri(rabbitMqSettings.Host) };
    return factory.CreateConnectionAsync().GetAwaiter().GetResult();
  })
  .AddSingleton(sp => new MongoClient(mongoConnectionString))
  .AddHealthChecks()
  .AddNpgSql(databaseConnectionString)
  .AddRedis(redisConnectionString)
  .AddRabbitMQ()
  .AddMongoDb()
  .AddKeyCloak(keyCloakHealthUrl);

ModuleServiceExtensions.AddModules(builder.Services, builder.Configuration);

var app = builder.Build();

// 1. Diagnostics and logging
app.UseLogContextTraceLogging();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();

// 3. Static files and FastEndpoints
app.UseStaticFiles();

// 4.Explicit routing
app.UseRouting();

// 5. CORS must run after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors("AllowUI");

// 6. Auth pipeline
app.UseAuthentication();
app.UseAuthorization();

// 7. FastEndpoints with audit processors
app.UseFastEndpoints(c =>
{
  c.Endpoints.ShortNames = true;
  c.Endpoints.Configurator = ep =>
  {
    ep.AddAuditProcessors();
  };
});

app.MapHub<DraftHub>("/drafts/hub");

if (app.Environment.IsDevelopment())
{
  app.UseOpenApi(c => c.Path = "/openapi/{documentname}.json");
  app.UseSwaggerGen();
  app.UseSwaggerUi();
  app.MapOpenApi();
  app.MapScalarApiReference();
  app.ApplyMigrations();
}

app.MapHealthChecks(
  "health",
  new HealthCheckOptions { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse }
);

await app.RunAsync();

public partial class Program
{
  protected Program() { }
}
