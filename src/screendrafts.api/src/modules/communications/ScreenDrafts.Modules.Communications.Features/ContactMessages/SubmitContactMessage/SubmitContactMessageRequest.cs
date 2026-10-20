namespace ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage;

internal sealed record SubmitContactMessageRequest
{
  public string Name { get; init; } = default!;
  public string Email { get; init; } = default!;
  public string Category { get; init; } = default!;
  public string Message { get; init; } = default!;
  public string TurnstileToken { get; init; } = default!;

  /// <summary>
  /// Honeypot. Rendered hidden in the form; real users never fill it. A non-empty value
  /// is treated as a bot and the submission is silently dropped.
  /// </summary>
  public string? Website { get; init; }
}
