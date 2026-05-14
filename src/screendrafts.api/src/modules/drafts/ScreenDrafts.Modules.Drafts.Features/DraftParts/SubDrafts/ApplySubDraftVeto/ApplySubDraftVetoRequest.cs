namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed record ApplySubDraftVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartPublicId { get; init; } = default!;

  [FromRoute(Name = "subDraftId")]
  public string SubDraftPublicId { get; init; } = default!;

  [FromRoute(Name = "playOrder")]
  public int PlayOrder { get; init; } = default!;

  public required string IssuerPublicId { get; init; }
  public required int IssuerKind  { get; init; }
}
