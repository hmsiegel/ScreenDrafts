namespace ScreenDrafts.Web.Extensions;
internal static class OpenApiExtensions
{
  internal static IServiceCollection ConfigureOpenApi(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddOpenApiDocument(config =>
    {
      config.PostProcess = document =>
      {
        document.Info = new NSwag.OpenApiInfo
        {
          Version = configuration.GetValueOrThrow<string>("OpenApi:Version"),
          Title = configuration.GetValueOrThrow<string>("OpenApi:Title"),
          Description = configuration.GetValueOrThrow<string>("OpenApi:Description"),
          TermsOfService = configuration.GetValueOrThrow<string>("OpenApi:TermsOfServiceUri"),
          Contact = new NSwag.OpenApiContact
          {
            Name = configuration.GetValueOrThrow<string>("OpenApi:Contact:Name"),
            Email = configuration.GetValueOrThrow<string>("OpenApi:Contact:Email"),
            Url = configuration.GetValueOrThrow<string>("OpenApi:Contact:Url")
          },
          License = new NSwag.OpenApiLicense
          {
            Name = configuration.GetValueOrThrow<string>("OpenApi:License:Name"),
            Url = configuration.GetValueOrThrow<string>("OpenApi:License:Url")
          }
        };
      };
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
