namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Picks.PlayPick;

internal sealed record PlayPickRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; init; } = default!;

  public int Position { get; init; }
  public int PlayOrder { get; init; }
  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
  public string MoviePublicId { get; init; } = default!;
  public int? TmdbId { get; init; }
  public string? MovieVersionName { get; init; }
}
