namespace ScreenDrafts.Common.Infrastructure.Identity;

public sealed class KeyCloakClient(HttpClient httpClient, IOptions<KeyCloakOptions> options)
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly KeyCloakOptions _options = options.Value;
  private static readonly string[] _value = ["UPDATE_PASSWORD"];

  public async Task<string> RegisterUserAsync(
    UserRepresentation user,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(user);
    Log.Information("HttpClient Base Address: {BaseAddress}", _httpClient.BaseAddress);
    Log.Information(
      "Registering user with the following values: Username: {Username}, First Name: {FirstName}, LastName: {LastName}, Email: {Email}",
      user.Username,
      user.FirstName,
      user.LastName,
      user.Email
    );

    var json = System.Text.Json.JsonSerializer.Serialize(user);
    Log.Information("Registering use with the following JSON payload: {JsonPayload}", json);

    var httpResponseMessage = await _httpClient.PostAsJsonAsync("users", user, cancellationToken);

    if (!httpResponseMessage.IsSuccessStatusCode)
    {
      var body = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
      Log.Error(
        "Failed to register user. Status Code: {StatusCode}, Body: {Body}",
        httpResponseMessage.StatusCode,
        body
      );
    }

    httpResponseMessage.EnsureSuccessStatusCode();

    return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
  }

  // Verifies the current password via Resource Owner Password grant,
  // then resets to the new password via the Admin API.
  // The HttpClient base address is already the admin URL — reset-password
  // is called at users/{identityId}/reset-password relative to that.
  public static async Task VerifyPasswordAsync(
    string userEmail,
    string currentPassword,
    KeyCloakOptions options,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(options);
    // Use a separate HttpClient for the token endpoint (different base URL).
    using var http = new HttpClient();
    using var tokenRequest = new FormUrlEncodedContent(
      new Dictionary<string, string>
      {
        ["grant_type"] = "password",
        ["client_id"] = options.PublicClientId,
        ["username"] = userEmail,
        ["password"] = currentPassword,
      }
    );

    var tokenRes = await http.PostAsync(new Uri(options.TokenUrl), tokenRequest, cancellationToken);
    if (!tokenRes.IsSuccessStatusCode)
    {
      throw new InvalidOperationException("Current password verification failed.");
    }
  }

  public async Task ResetPasswordAsync(
    string identityId,
    string newPassword,
    CancellationToken cancellationToken = default
  )
  {
    var body = new CredentialRepresentation("password", newPassword, false);

    var response = await _httpClient.PutAsJsonAsync(
      $"users/{identityId}/reset-password",
      body,
      cancellationToken
    );

    if (!response.IsSuccessStatusCode)
    {
      var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
      Log.Error(
        "Failed to reset password for identity {IdentityId}. Status: {Status}, Body: {Body}",
        identityId,
        response.StatusCode,
        responseBody
      );
    }

    response.EnsureSuccessStatusCode();
  }

  public async Task SendPasswordResetEmailAsync(
    string identityId,
    CancellationToken cancellationToken = default
  )
  {
    var actionsUrl =
      $"users/{identityId}/execute-actions-email"
      + $"?client_id={_options.PublicClientId}"
      + $"&redirect_uri={Uri.EscapeDataString("http://localhost:3005")}";
    var body = System.Text.Json.JsonSerializer.Serialize(_value);

    using var request = new HttpRequestMessage(HttpMethod.Put, actionsUrl)
    {
      Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json"),
    };

    var response = await _httpClient.SendAsync(request, cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
      Log.Error(
        "Failed to send password reset email for {IdentityId}. Status: {Status}, Body: {Body}",
        identityId,
        response.StatusCode,
        responseBody
      );
    }

    response.EnsureSuccessStatusCode();
  }

  private static string ExtractIdentityIdFromLocationHeader(HttpResponseMessage httpResponseMessage)
  {
    const string usersSegmentName = "users/";

    var locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

    if (locationHeader is null)
    {
      throw new InvalidOperationException("The location header is missing.");
    }

    var userSegmentValueIndex = locationHeader.IndexOf(
      usersSegmentName,
      StringComparison.InvariantCultureIgnoreCase
    );

    var identityId = locationHeader[(userSegmentValueIndex + usersSegmentName.Length)..];

    return identityId;
  }
}
