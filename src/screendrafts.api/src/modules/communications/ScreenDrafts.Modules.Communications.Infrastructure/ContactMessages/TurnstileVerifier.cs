namespace ScreenDrafts.Modules.Communications.Infrastructure.ContactMessages;

internal sealed partial class TurnstileVerifier(
  HttpClient httpClient,
  IOptions<TurnstileOptions> options,
  ILogger<TurnstileVerifier> logger
) : ITurnstileVerifier
{
  private readonly HttpClient _httpClient = httpClient;
  private readonly TurnstileOptions _options = options.Value;
  private readonly ILogger<TurnstileVerifier> _logger = logger;

  public async Task<bool> VerifyAsync(
    string token,
    string? remoteIpAddress,
    CancellationToken cancellationToken = default
  )
  {
    if (string.IsNullOrWhiteSpace(token))
    {
      return false;
    }

    if (string.IsNullOrWhiteSpace(_options.SecretKey))
    {
      LogSecretMissing(_logger);
      return false;
    }

    var form = new Dictionary<string, string>
    {
      ["secret"] = _options.SecretKey,
      ["response"] = token,
    };

    if (!string.IsNullOrWhiteSpace(remoteIpAddress))
    {
      form["remoteip"] = remoteIpAddress;
    }

    try
    {
      using var content = new FormUrlEncodedContent(form);
      using var response = await _httpClient.PostAsync(
        new Uri(_options.VerifyUrl),
        content,
        cancellationToken
      );

      if (!response.IsSuccessStatusCode)
      {
        LogUnexpectedStatus(_logger, (int)response.StatusCode);
        return false;
      }

      await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
      using var document = await JsonDocument.ParseAsync(
        stream,
        cancellationToken: cancellationToken
      );

      var root = document.RootElement;

      if (
        !root.TryGetProperty("success", out var success)
        || success.ValueKind != JsonValueKind.True
      )
      {
        return false;
      }

      var allowed = ParseAllowedHostnames(_options.AllowedHostnames);
      if (allowed.Length == 0)
      {
        return true;
      }

      var hostname =
        root.TryGetProperty("hostname", out var hostElement)
        && hostElement.ValueKind == JsonValueKind.String
          ? hostElement.GetString()
          : null;

      if (hostname is not null && allowed.Contains(hostname, StringComparer.OrdinalIgnoreCase))
      {
        return true;
      }

      LogHostnameMismatch(_logger, hostname);
      return false;
    }
#pragma warning disable CA1031 // Intentional: fail closed on any verification problem.
    catch (Exception ex)
      when (ex is HttpRequestException or System.Text.Json.JsonException
        || (ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
      )
#pragma warning restore CA1031
    {
      LogVerificationFailed(_logger, ex);
      return false;
    }
  }

  private static string[] ParseAllowedHostnames(string? value) =>
    string.IsNullOrWhiteSpace(value)
      ? []
      : value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

  [LoggerMessage(
    EventId = 3004,
    Level = LogLevel.Warning,
    Message = "Turnstile token hostname {Hostname} is not in the allowed list."
  )]
  private static partial void LogHostnameMismatch(
    ILogger<TurnstileVerifier> logger,
    string? hostname
  );

  [LoggerMessage(
    EventId = 3001,
    Level = LogLevel.Error,
    Message = "Turnstile secret key is not configured; rejecting contact form submission."
  )]
  private static partial void LogSecretMissing(ILogger<TurnstileVerifier> logger);

  [LoggerMessage(
    EventId = 3002,
    Level = LogLevel.Warning,
    Message = "Turnstile verification returned unexpected HTTP status {StatusCode}."
  )]
  private static partial void LogUnexpectedStatus(
    ILogger<TurnstileVerifier> logger,
    int statusCode
  );

  [LoggerMessage(
    EventId = 3003,
    Level = LogLevel.Warning,
    Message = "Turnstile verification request failed."
  )]
  private static partial void LogVerificationFailed(
    ILogger<TurnstileVerifier> logger,
    Exception exception
  );
}
