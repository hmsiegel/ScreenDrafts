namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.GamePlay.GetDraftPartGamePlay;

internal sealed record GameplayParticipantResponse
{
  public Guid ParticipantId { get; init; }
  public string? ParticipantPublicId { get; init; }
  public int ParticipantKind { get; init; }
  public string ParticipantName { get; init; } = default!;
  public int VetoTokensRemaining { get; init; }
  public int OverrideTokensRemaining { get; init; }

  /// <summary>
  /// Vetoes carried in from the previous part/draft, distinct from the
  /// part's flat starting allotment. Exposed separately from
  /// VetoTokensRemaining (a net total) because consumers like the patter
  /// drawer need to say "you're rolling in a veto" specifically — a fact
  /// about provenance that the aggregate total cannot recover once
  /// starting/rolling-in/awarded/used are all summed together.
  /// </summary>
  public int VetoesRollingIn { get; init; }

  /// <summary>
  /// Veto overrides carried in from the previous part/draft. See
  /// VetoesRollingIn for why this is exposed separately from
  /// OverrideTokensRemaining.
  /// </summary>
  public int VetoOverridesRollingIn { get; init; }
}
