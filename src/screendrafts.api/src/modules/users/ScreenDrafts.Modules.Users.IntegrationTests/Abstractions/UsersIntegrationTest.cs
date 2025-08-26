namespace ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;

[Collection(nameof(UsersIntegrationTestCollection))]
public abstract class UsersIntegrationTest : BaseIntegrationTest<UsersDbContext>
{
  private readonly KeyCloakOptions _keyCloakOptions;

  protected UsersIntegrationTest(UsersIntegrationTestWebAppFactory factory) : base(factory)
  {
    _keyCloakOptions = ServiceScope.ServiceProvider.GetRequiredService<IOptions<KeyCloakOptions>>().Value;
  }

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE 
        users.user_roles,
        users.users
      RESTART IDENTITY CASCADE;
      """);
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
}
