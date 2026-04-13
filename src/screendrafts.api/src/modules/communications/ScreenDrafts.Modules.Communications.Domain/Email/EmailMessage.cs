namespace ScreenDrafts.Modules.Communications.Domain.Email;

public sealed record EmailMessage
{
  public required string ToAddress { get; init; }
  public required string ToName { get; init; }
  public required string Subject { get; init; }
  public required string HtmlBody { get; init; }
}
