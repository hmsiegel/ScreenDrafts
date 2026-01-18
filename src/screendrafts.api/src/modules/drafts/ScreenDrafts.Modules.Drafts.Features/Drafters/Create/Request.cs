namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed record Request
{
  public string PersonId { get; init; } = default!;
}
