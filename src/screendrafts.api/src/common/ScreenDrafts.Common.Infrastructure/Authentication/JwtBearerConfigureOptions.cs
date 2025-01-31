namespace ScreenDrafts.Common.Infrastructure.Authentication;

internal sealed class JwtBearerConfigureOptions(IConfiguration configuration)
  : IConfigureNamedOptions<JwtBearerOptions>
{
  private readonly IConfiguration _configuration = configuration;
  private const string ConfigurationSectionName = "Authentication";

  public void Configure(string? name, JwtBearerOptions options)
  {
    Configure(options);
  }

  public void Configure(JwtBearerOptions options)
  {
    _configuration.GetSection(ConfigurationSectionName).Bind(options);
  }
}
