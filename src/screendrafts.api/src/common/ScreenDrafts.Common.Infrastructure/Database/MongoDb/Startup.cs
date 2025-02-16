namespace ScreenDrafts.Common.Infrastructure.Database.MongoDb;

internal static class Startup
{
  internal static IServiceCollection AddMongoDb(
    this IServiceCollection services,
    string mongoConnectionString)
  {
    var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

    mongoClientSettings.ClusterConfigurator = c => c.Subscribe(
        new DiagnosticsActivityEventSubscriber(
            new InstrumentationOptions
            {
              CaptureCommandText = true,
            }));

    services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoClientSettings));

    BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

    return services;
  }
}
