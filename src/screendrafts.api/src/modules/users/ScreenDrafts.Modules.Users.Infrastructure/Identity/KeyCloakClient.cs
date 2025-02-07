namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;

internal sealed class KeyCloakClient(HttpClient httpClient)
{
  private readonly HttpClient _httpClient = httpClient;

  internal async Task<string> RegisterUserAsync(UserRepresentation user, CancellationToken cancellationToken = default)
  {
    var httpResposeMessage = await _httpClient.PostAsJsonAsync(
      "users",
      user,
      cancellationToken);

    httpResposeMessage.EnsureSuccessStatusCode();

    return ExtractIdentityIdFromLocationHeader(httpResposeMessage);
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
