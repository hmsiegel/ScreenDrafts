namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed record CreateDrafterRequest
{
  public string PersonId { get; init; } = default!;
}

