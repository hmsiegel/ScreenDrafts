using ScreenDrafts.Common.Infrastructure.Identity;

namespace ScreenDrafts.Modules.Users.IntegrationTests.Abstractions;

[Collection(nameof(UsersIntegrationTestCollection))]
public abstract class UsersIntegrationTest : BaseIntegrationTest<UsersDbContext>
{
  private readonly KeyCloakOptions _keyCloakOptions;

  protected UsersIntegrationTest(UsersIntegrationTestWebAppFactory factory)
    : base(factory)
  {
    _keyCloakOptions = ServiceScope
      .ServiceProvider.GetRequiredService<IOptions<KeyCloakOptions>>()
      .Value;
  }

  protected override async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE
        users.user_permissions,
        users.email_bootstrap_claims,
        users.email_change_tokens,
        users.users
      RESTART IDENTITY CASCADE;
      """
    );

    await DbContext.Database.ExecuteSqlRawAsync(
      """
      TRUNCATE TABLE administration.user_roles RESTART IDENTITY CASCADE;
      """
    );
  }

  protected async Task<GetByUserIdResponse> RegisterUserAsync(
    string? email = null,
    string password = "Test@123456"
  )
  {
    var registerResult = await Sender.Send(
      new RegisterUserCommand
      {
        Email = email ?? Faker.Internet.Email(),
        Password = password,
        FirstName = Faker.Name.FirstName(),
        LastName = Faker.Name.LastName(),
      },
      TestContext.Current.CancellationToken
    );

    var userResult = await Sender.Send(
      new GetByUserIdQuery(registerResult.Value),
      TestContext.Current.CancellationToken
    );

    return userResult.Value;
  }

  protected async Task GrantPermissionAsync(string publicId, string permissionCode)
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      INSERT INTO users.user_permissions (user_id, permission_code)
      SELECT id, {1}
      FROM users.users
      WHERE public_id = {0}
      """,
      publicId,
      permissionCode
    );
  }

  protected async Task GrantRoleAsync(string publicId, string roleName)
  {
    // administration.user_roles.role_name has an FK to administration.roles(name).
    await DbContext.Database.ExecuteSqlRawAsync(
      """
      INSERT INTO administration.roles (name)
      VALUES ({0})
      ON CONFLICT (name) DO NOTHING
      """,
      roleName
    );

    await DbContext.Database.ExecuteSqlRawAsync(
      """
      INSERT INTO administration.user_roles (user_id, role_name)
      SELECT id, {1}
      FROM users.users
      WHERE public_id = {0}
      """,
      publicId,
      roleName
    );
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
      new("password", password),
    };

    using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);

    using var authRequest = new HttpRequestMessage(
      HttpMethod.Post,
      new Uri(_keyCloakOptions.TokenUrl)
    );
    authRequest.Content = authRequestContent;

    using HttpResponseMessage authorizationResponse = await client.SendAsync(authRequest);

    authorizationResponse.EnsureSuccessStatusCode();

    var authToken = await authorizationResponse.Content.ReadFromJsonAsync<AuthToken>(
      TestContext.Current.CancellationToken
    );

    return authToken!.AccessToken;
  }

  internal sealed class AuthToken
  {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;
  }
}
