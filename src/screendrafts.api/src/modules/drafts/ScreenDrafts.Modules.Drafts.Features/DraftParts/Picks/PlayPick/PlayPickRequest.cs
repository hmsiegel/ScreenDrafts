namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed record PlayPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  public int Position { get; init; }
  public int PlayOrder { get; init; }
  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
  public Guid MovieId { get; init; }
  public string? MovieVersionName { get; init; }
}
