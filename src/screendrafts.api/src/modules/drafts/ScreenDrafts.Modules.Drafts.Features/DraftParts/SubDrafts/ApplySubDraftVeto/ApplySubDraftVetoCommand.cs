namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.ApplySubDraftVeto;

internal sealed record ApplySubDraftVetoCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string SubDraftPublicId { get; init; }
  public required int PlayOrder { get; init; }
  public required string IssuerPublicId { get; init; }
  public required ParticipantKind IssuerKind { get; init; }
  public string? ActedbyPublicId { get; init; }
}
