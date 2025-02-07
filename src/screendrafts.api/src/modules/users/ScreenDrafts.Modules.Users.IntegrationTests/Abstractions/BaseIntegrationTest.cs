namespace ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public class BaseIntegrationTest : IDisposable
{
  private bool _disposedValue;
  protected static readonly Faker Faker = new();
  private readonly IServiceScope _serviceScope;
  private readonly KeyCloakOptions _keyCloakOptions;
  protected readonly ISender Sender;
  protected readonly HttpClient HttpClient;
  protected readonly UsersDbContext DbContext;


  protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    _serviceScope = factory.Services.CreateScope();
    Sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
    HttpClient = factory.CreateClient();
    _keyCloakOptions = _serviceScope.ServiceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
    DbContext = _serviceScope.ServiceProvider.GetRequiredService<UsersDbContext>();
  }

  protected async Task<string> GetAccessTokenAsync(string email, string password)
  {
    using var client = new HttpClient();

    var authRequestParameters = new KeyValuePair<string, string>[]
    {
      new("client_id", _keyCloakOptions.PublicClientId),
      new("scope", "openid"),
      new("grant_type", "password"),
      new("username", email),
      new("password", password)
    };

    using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);

    using var authRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_keyCloakOptions.TokenUrl));
    authRequest.Content = authRequestContent;

    using HttpResponseMessage authorizationResponse = await client.SendAsync(authRequest);

    authorizationResponse.EnsureSuccessStatusCode();

    var authToken = await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>();

    return authToken!.AccessToken;
  }

  internal sealed class AuthToken
  {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;
  }


  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _serviceScope.Dispose();
        HttpClient.Dispose();
        DbContext.Dispose();
      }

      _disposedValue = true;
    }
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
}
