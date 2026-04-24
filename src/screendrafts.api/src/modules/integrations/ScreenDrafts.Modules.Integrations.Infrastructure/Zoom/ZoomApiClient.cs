namespace ScreenDrafts.Modules.Integrations.Infrastructure.Zoom;

internal sealed partial class ZoomApiClient(
  HttpClient httpClient,
  IOptions<ZoomSettings> settings,
  ILogger<ZoomApiClient> logger,
  IDateTimeProvider timeProvider) : IZoomApiClient
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly ZoomSettings _settings = settings.Value;
  private readonly ILogger<ZoomApiClient> _logger = logger;
  private readonly IDateTimeProvider _timeProvider = timeProvider;

  private string? _accessToken;
  private DateTimeOffset _tokenExpiration = DateTimeOffset.MinValue;
  private readonly SemaphoreSlim _tokenLock = new(1, 1);

  // ---- Recording Management ----

  /// <summary>
  /// Starts a cloud recording for an active Video SDK session.
  /// </summary>
  public async Task StartRecordingAsync(string sessionName, CancellationToken cancellationToken = default)
  {
    var accessToken = await GetAccessTokenAsync(cancellationToken);
    using var request = new HttpRequestMessage(
      HttpMethod.Patch,
      $"https://api.zoom.us/v2/videosdk/sessions/{Uri.EscapeDataString(sessionName)}/recordings/start");

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await _retryPipeline.ExecuteAsync(
      async ct =>
        await _httpClient.SendAsync(
          request: request,
          cancellationToken: cancellationToken),
      cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync(cancellationToken);
      LogFailedToStartZoomRecording(_logger, sessionName, response.StatusCode, body);
    }
    else
    {
      var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
      LogFailedToStartZoomRecording(_logger, sessionName, response.StatusCode, errorContent);
      response.EnsureSuccessStatusCode();
    }
  }

  public async Task StopRecordingAsync(string sessionName, CancellationToken cancellationToken = default)
  {
    var accessToken = await GetAccessTokenAsync(cancellationToken);

    using var request = new HttpRequestMessage(
      HttpMethod.Patch,
      $"https://api.zoom.us/v2/videosdk/sessions/{Uri.EscapeDataString(sessionName)}/recordings/stop");

    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await _retryPipeline.ExecuteAsync(
      async ct =>
        await _httpClient.SendAsync(
          request: request,
          cancellationToken: cancellationToken),
      cancellationToken);

    if (!response.IsSuccessStatusCode)
    {
      var body = await response.Content.ReadAsStringAsync(cancellationToken);
      LogFailedToStopZoomRecording(_logger, sessionName, response.StatusCode, body);

      response.EnsureSuccessStatusCode();
    }
  }

  private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
  {
    if (_accessToken is not null && _timeProvider.UtcNow < _tokenExpiration.AddSeconds(-60))
    {
      return _accessToken;
    }

    await _tokenLock.WaitAsync(cancellationToken);

    try
    {
      if (_accessToken is not null && _timeProvider.UtcNow < _tokenExpiration.AddSeconds(-60))
      {
        return _accessToken;
      }

      var credentials = Convert.ToBase64String(
        Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

      using var request = new HttpRequestMessage(
        HttpMethod.Post,
        $"https://zoom.us/oauth/token?grant_type=account_credentials?account_id={_settings.AccountId}");


      request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

      var response = await _httpClient.SendAsync(request, cancellationToken);
      response.EnsureSuccessStatusCode();

      var json = await response.Content.ReadAsStringAsync(cancellationToken);
      var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<ZoomTokenResponse>(json, SerializerOptions.ZoomDefault);

      _accessToken = tokenResponse!.AccessToken;
      _tokenExpiration = _timeProvider.UtcNow.AddSeconds(tokenResponse.ExpiresIn);

      return _accessToken;

    }
    finally
    {
      _tokenLock.Release();
    }
  }

  public void Dispose()
  {
    _tokenLock.Dispose();
    _httpClient.Dispose();
  }

  private sealed class ZoomTokenResponse
  {
    public string AccessToken { get; init; } = null!;
    public int ExpiresIn { get; init; } = -60;
  }

  private static readonly ResiliencePipeline<HttpResponseMessage> _retryPipeline =
    new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddRetry(new Polly.Retry.RetryStrategyOptions<HttpResponseMessage>
    {
      MaxRetryAttempts = 3,
      Delay = TimeSpan.FromSeconds(2),
      BackoffType = DelayBackoffType.Exponential,
      ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<HttpRequestException>()
        .HandleResult(r => (int)r.StatusCode >= 500)
    })
    .Build();

  [LoggerMessage(
    EventId = 1000,
    Level = LogLevel.Error,
    Message = "Failed to start recording for Zoom session {SessionName}. Status: {StatusCode}, Response: {Response}")]
  private static partial void LogFailedToStartZoomRecording(ILogger logger, string sessionName, HttpStatusCode statusCode, string response);

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "Failed to stop recording for Zoom session {SessionName}. Status: {StatusCode}, Response: {Response}")]
  private static partial void LogFailedToStopZoomRecording(ILogger logger, string sessionName, HttpStatusCode statusCode, string response);
}
