namespace ScreenDrafts.Modules.Users.Infrastructure.Identity;
internal sealed class KeyCloakAuthDelegatingHandler(IOptions<KeyCloakOptions> options) : DelegatingHandler
{
  private readonly KeyCloakOptions _options = options.Value;

  protected override async Task<HttpResponseMessage> SendAsync(
    HttpRequestMessage request,
    CancellationToken cancellationToken)
  {
    var authorizationToken = await GetAuthorizationTokenAsync(cancellationToken);

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authorizationToken.AccessToken);

    var httpResponseMessage = await base.SendAsync(request, cancellationToken);

    if (!httpResponseMessage.IsSuccessStatusCode)
    {
      var body = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);
      Log.Error(
        "Failed to send request to KeyCloak. Status Code: {StatusCode}, Body: {Body}, RequestUri: {RequestUri}",
        httpResponseMessage.StatusCode,
        body,
        request.RequestUri);
    }

    return httpResponseMessage;
  }

  private async Task<AuthToken> GetAuthorizationTokenAsync(CancellationToken cancellationToken)
  {
    var authRequestParameters = new KeyValuePair<string, string>[]
    {
      new("client_id", _options.ConfidentialClientId),
      new("client_secret", _options.ConfidentialClientSecret),
      new("scope", "openid"),
      new("grant_type", "client_credentials"),
    };

    using var authRequestContent = new FormUrlEncodedContent(authRequestParameters);

    using var authRequest = new HttpRequestMessage(HttpMethod.Post, new Uri(_options.TokenUrl))
    {
      Content = authRequestContent,
    };

    using HttpResponseMessage authorizationMessage = await base.SendAsync(authRequest, cancellationToken);

    authorizationMessage.EnsureSuccessStatusCode();

    return (await authorizationMessage.Content.ReadFromJsonAsync<AuthToken>(cancellationToken: cancellationToken))!;
  }

  internal sealed class AuthToken
  {
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = default!;
  }
}
