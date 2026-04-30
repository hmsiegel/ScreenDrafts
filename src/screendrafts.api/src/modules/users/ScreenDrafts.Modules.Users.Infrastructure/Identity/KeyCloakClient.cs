namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakClient(HttpClient httpClient)
{
  private readonly HttpClient _httpClient = httpClient;

  internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
  {
    Log.Information("HttpClient Base Address: {BaseAddress}", _httpClient.BaseAddress);
    Log.Information(
      "Registering user with the following values: Username: {Username}, First Name: {FirstName}, LastName: {LastName}, Email: {Email}",
      user.Username,
      user.FirstName,
      user.LastName,
      user.Email);

    var json = System.Text.Json.JsonSerializer.Serialize(user);
    Log.Information("Registering use with the following JSON payload: {JsonPayload}", json);

    var httpResponseMessage = await _httpClient.PostAsJsonAsync(
      "users",
      user,
      cancellationToken);

    if (!httpResponseMessage.IsSuccessStatusCode)
    {
      var body = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
      Log.Error(
        "Failed to register user. Status Code: {StatusCode}, Body: {Body}",
        httpResponseMessage.StatusCode,
        body);
    }

    httpResponseMessage.EnsureSuccessStatusCode();

    return ExtractIdentityIdFromLocationHeader(httpResponseMessage);
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
      StringComparison.InvariantCultureIgnoreCase);

    var identityId = locationHeader[(userSegmentValueIndex + usersSegmentName.Length)..];

    return identityId;
  }
}
