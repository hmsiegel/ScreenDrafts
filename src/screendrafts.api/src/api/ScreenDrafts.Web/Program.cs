var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var configuration = builder.Configuration;

var databaseConnectionString = builder.Services.AddPostgresDatabase(configuration);
var redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache")!;
var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionStringOrThrow("Queue"));
var mongoConnectionString = builder.Configuration.GetConnectionStringOrThrow("Mongo")!;

builder.Services.AddApplication(AssemblyReferences.ApplicationAssemblies);

builder.Services.AddInfrastructure(
  configuration,
  DiagnosticsConfig.ServiceName,
  [
    DraftsModule.ConfigureConsumers,
    IntegrationsModule.ConfigureConsumers,
    MoviesModule.ConfigureConsumers
  ],
  rabbitMqSettings,
  redisConnectionString,
  mongoConnectionString,
  AssemblyReferences.InfrastructureAssemblies);

builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = AssemblyReferences.PresentationAssemblies;
})
  .SwaggerDocument(o => o.ShortSchemaNames = true);

builder.Services.ConfigureOpenApi(builder.Configuration);

builder.Configuration.AddModuleConfiguration(ModuleReferences.Modules);

var keyCloakHealthUrl = builder.Configuration.GetKeyCloakHealthUrl();

builder.Services
  .AddSingleton(sp =>
  {
    var factory = new ConnectionFactory
    {
      Uri = new Uri(rabbitMqSettings.Host)
    };
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

app.UseFastEndpoints(c =>
{
  c.Endpoints.ShortNames = true;
});

if (app.Environment.IsDevelopment())
{
  app.UseOpenApi(c => c.Path = "/openapi/{documentname}.json");
  app.UseSwaggerGen();
  app.UseSwaggerUi();
  app.MapOpenApi();
  app.MapScalarApiReference();
  app.ApplyMigrations();
}

app.MapHealthChecks("health", new HealthCheckOptions
{
  ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseLogContextTraceLogging();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseCors("AllowUI");
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();


public partial class Program
{
  protected Program()
  {
  }
}
