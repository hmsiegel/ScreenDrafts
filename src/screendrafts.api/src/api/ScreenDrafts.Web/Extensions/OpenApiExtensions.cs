namespace ScreenDrafts.Web.Extensions;
internal static class OpenApiExtensions
{
  internal static IServiceCollection ConfigureOpenApi(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOpenApiDocument(config =>
    {
      config.Title = configuration.GetValueOrThrow<string>("OpenApi:Title");
      config.Description = configuration.GetValueOrThrow<string>("OpenApi:Description");
      config.Version = configuration.GetValueOrThrow<string>("OpenApi:Version");

      config.SchemaSettings.GenerateKnownTypes = true;

    });

    services.AddOpenApi(options =>
    {
      options.AddDocumentTransformer((document, context, cancellationToken) =>
      {
        document.Info.Version = configuration.GetValueOrThrow<string>("OpenApi:Version");
        document.Info.Title = configuration.GetValueOrThrow<string>("OpenApi:Title");
        document.Info.Description = configuration.GetValueOrThrow<string>("OpenApi:Description");
        document.Info.TermsOfService = new Uri(configuration.GetValueOrThrow<string>("OpenApi:TermsOfServiceUri"));
        document.Info.Contact = new OpenApiContact
        {
          Name = configuration.GetValueOrThrow<string>("OpenApi:Contact:Name"),
          Email = configuration.GetValueOrThrow<string>("OpenApi:Contact:Email"),
          Url = new Uri(configuration.GetValueOrThrow<string>("OpenApi:Contact:Url"))
        };
        document.Info.License = new OpenApiLicense
        {
          Name = configuration.GetValueOrThrow<string>("OpenApi:License:Name"),
          Url = new Uri(configuration.GetValueOrThrow<string>("OpenApi:License:Url"))
        };
        return Task.CompletedTask;
      });
      options.AddDocumentTransformer<BearerSecuritySchemeTransformes>();
    });
    return services;
  }
}
