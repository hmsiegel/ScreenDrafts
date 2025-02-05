﻿namespace ScreenDrafts.Web.Extensions;

internal sealed class BearerSecuritySchemeTransformes(IAuthenticationSchemeProvider authenticationSchemeProvider)
  : IOpenApiDocumentTransformer
{
  private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider = authenticationSchemeProvider;

  public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
  {
    var authenticationSchemes = await _authenticationSchemeProvider.GetAllSchemesAsync();
    if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
    {
      // Add the security scheme at the document level
      var requirements = new Dictionary<string, OpenApiSecurityScheme>
      {
        ["Bearer"] = new OpenApiSecurityScheme
        {
          Type = SecuritySchemeType.Http,
          Scheme = "bearer", // "bearer" refers to the header name here
          In = ParameterLocation.Header,
          BearerFormat = "JWT"
        }
      };
      document.Components ??= new OpenApiComponents();
      document.Components.SecuritySchemes = requirements;

      // Apply it as a requirement for all operations
      foreach (var operation in document.Paths.Values.SelectMany(path => path.Operations))
      {
        operation.Value.Security.Add(new OpenApiSecurityRequirement
        {
          [new OpenApiSecurityScheme
          {
            Reference = new OpenApiReference
            {
              Id = "Bearer",
              Type = ReferenceType.SecurityScheme
            }
          }] = Array.Empty<string>()
        });
      }
    }
  }
}
