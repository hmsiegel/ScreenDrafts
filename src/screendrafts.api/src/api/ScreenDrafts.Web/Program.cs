using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
  config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureOpenApi(builder.Configuration);

var databaseConnectionString = builder.Configuration.GetConnectionStringOrThrow("Database")!;
var redisConnectionString = builder.Configuration.GetConnectionStringOrThrow("Cache")!;
var rabbitMqSettings = new RabbitMqSettings(builder.Configuration.GetConnectionStringOrThrow("Queue"));
var mongoConnectionString = builder.Configuration.GetConnectionStringOrThrow("Mongo")!;

builder.Services.AddApplication(AssemblyReferences.ApplicationAssemblies);
builder.Services.AddInfrastructure(
  DiagnosticsConfig.ServiceName,
  [DraftsModule.ConfigureConsumers],
  rabbitMqSettings,
  databaseConnectionString,
  redisConnectionString,
  mongoConnectionString);

builder.Services.AddFastEndpoints(opt =>
{
  opt.Assemblies = AssemblyReferences.PresentationAssemblies;
});

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

await app.UseDraftsModuleAsync();

app.UseLogContextTraceLogging();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

await app.RunAsync();


public partial class Program
{
  protected Program()
  {
  }
}
