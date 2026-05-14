namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed record CreateDraftPartRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public int MinimumPosition { get; init; }
  public int MaximumPosition { get; init; }
}
