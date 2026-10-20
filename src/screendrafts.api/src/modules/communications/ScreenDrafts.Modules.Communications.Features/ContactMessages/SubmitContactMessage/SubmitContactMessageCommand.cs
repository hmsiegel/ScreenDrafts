namespace ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage;

public sealed record SubmitContactMessageCommand : ICommand
{
  public string Name { get; init; } = default!;
  public string Email { get; init; } = default!;

  /// <summary>One of the ContactCategory names: Bug, Question, Feedback, AccountHelp.</summary>
  public string Category { get; init; } = default!;
  public string Message { get; init; } = default!;
  public string TurnstileToken { get; init; } = default!;

  /// <summary>Signed-in sender's public ID (u_...), if any.</summary>
  public string? UserPublicId { get; init; }

  public string? RemoteIpAddress { get; init; }
}
