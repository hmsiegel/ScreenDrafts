namespace ScreenDrafts.Modules.Integrations.Features.Zoom.ZoomWebhook;

internal sealed class Endpoint(IZoomWebhookSignatureVerifier signatureVerifier)
  : ScreenDraftsEndpointWithoutRequest
{
  private readonly IZoomWebhookSignatureVerifier _signatureVerifier = signatureVerifier;
  public override void Configure()
  {
    Post(ZoomRoutes.Webhook);
    Description(x =>
    {
      x.WithTags(IntegrationsOpenApi.Tags.Zoom)
      .WithName(IntegrationsOpenApi.Names.Zoom_Webhook)
      .Produces(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    HttpContext.Request.EnableBuffering();

    using var reader = new StreamReader(
      HttpContext.Request.Body,
      Encoding.UTF8,
      leaveOpen: true);

    var rawBody = await reader.ReadToEndAsync(ct);
    HttpContext.Request.Body.Position = 0;

    var timestamp = HttpContext.Request.Headers["x-zm-request-timestamp"].FirstOrDefault() ?? string.Empty;
    var signature = HttpContext.Request.Headers["x-zm-signature"].FirstOrDefault() ?? string.Empty;

    if (!_signatureVerifier.IsValid(timestamp, signature, rawBody))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var payload = JsonSerializer.Deserialize<ZoomWebhookPayload>(
      rawBody,
      ZoomJsonOptions.ZoomDefault);

    if (payload is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    if (payload.Event == "endpoint.url_validation")
    {
      await HandleUrlValidationAsync(rawBody, ct);
      return;
    }

    if (payload.Event == "recording.completed")
    {
      await HandleRecordingCompletedAsync(payload, ct);
      return;
    }

    await Send.NoContentAsync(ct);

  }

  private async Task HandleRecordingCompletedAsync(ZoomWebhookPayload payload, CancellationToken ct)
  {
    var obj = payload.Payload?.WebhookObject;

    if (obj is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var recordingFile = (obj.RecordingFiles?.Files ?? [])
      .Where(f => f.Status == "completed")
      .Select(f => new ZoomRecordingFileModel(
        zoomFileId: f.Id,
        fileType: f.FileType,
        playUrl: f.PlayUrl,
        downloadUrl: f.DownloadUrl,
        recordingStart: DateTimeOffset.Parse(f.RecordingStart, CultureInfo.InvariantCulture),
        recordingEnd: DateTimeOffset.Parse(f.RecordingEnd, CultureInfo.InvariantCulture),
        fileSizeBytes: f.FileSize))
      .ToList();

    var command = new ProcessZoomRecordingCommand
    {
      ZoomMeetingId = obj.Uuid,
      MeetingTopic = obj.Topic,
      MeetingStartTime = obj.StartTime,
      MeetingDurationMinutes = obj.Duration,
      RecordingFiles = recordingFile
    };

    var result = await Sender.Send(command, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status500InternalServerError, ct);
    }

    await Send.OkAsync(result, ct);
  }

  private async Task HandleUrlValidationAsync(string rawBody, CancellationToken ct)
  {
    var challengePayload = JsonSerializer.Deserialize<ZoomUrlValidationPayload>(
      rawBody,
      ZoomJsonOptions.ZoomDefault);

    if (challengePayload?.Payload?.PlainToken is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var plainToken = challengePayload.Payload.PlainToken;
    var keyBytes = Encoding.UTF8.GetBytes(_signatureVerifier.SecretToken);
    var tokenBytes = Encoding.UTF8.GetBytes(plainToken);

    using var hmac = new HMACSHA256(keyBytes);
    var hashBytes = hmac.ComputeHash(tokenBytes);
    var encryptedToken = Convert.ToBase64String(hashBytes).ToLowerInvariant();

    await Send.OkAsync(new ZoomUrlValidationResponse
    {
      PlainToken = plainToken,
      EncryptedToken = encryptedToken
    }, ct);
  }

  internal static class ZoomJsonOptions
  {

    public static readonly JsonSerializerOptions ZoomDefault = new()
    {
      PropertyNameCaseInsensitive = true,
      PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    };
  }
}

internal sealed class ZoomUrlValidationResponse
{
  public required string PlainToken { get; set; } 
  public required string EncryptedToken { get; set; }
}
