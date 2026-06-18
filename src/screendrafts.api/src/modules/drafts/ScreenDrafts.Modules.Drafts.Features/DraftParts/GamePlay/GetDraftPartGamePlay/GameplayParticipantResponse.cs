namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayParticipantResponse
{
  public Guid ParticipantId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
  public string ParticipantName { get; init; } = default!;
  public int VetoTokensRemaining { get; init; }
  public int OverrideTokensRemaining { get; init; }
}
