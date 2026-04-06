namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed record ApplySubDraftVetoRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartPublicId { get; init; }

  [FromRoute(Name = "subDraftId")]
  public required string SubDraftPublicId { get; init; }

  [FromRoute(Name = "playOrder")]
  public required int PlayOrder { get; init; }

  public required string IssuerPublicId { get; init; }
  public required int IssuerKind  { get; init; }
}
