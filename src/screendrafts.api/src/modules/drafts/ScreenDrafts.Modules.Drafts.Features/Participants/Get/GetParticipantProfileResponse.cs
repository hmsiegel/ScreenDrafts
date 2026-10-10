namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record GetParticipantProfileResponse
{
  public required string PersonPublicId { get; init; }
  public required string DisplayName { get; init; }
  public string? Biography { get; init; }
  public string? Location { get; init; }
  public bool IsCommissioner { get; init; }
  public HonorificResponse? Honorific { get; init; }
  public SocialHandles? SocialHandles { get; init; }

  /// <summary>True when the stats and history below cover Patreon drafts only.</summary>
  public bool IsPatreonView { get; init; }

  /// <summary>
  /// True when the caller may see Patreon drafts and this person has appeared in one
  /// (as drafter or host). Drives the main feed / Patreon toggle.
  /// </summary>
  public bool HasPatreonDrafts { get; init; }

  /// <summary>Null when the person has no drafter profile.</summary>
  public DrafterStatsResponse? DrafterStats { get; init; }

  /// <summary>Null when the person has no host profile.</summary>
  public HostStatsResponse? HostStats { get; init; }

  public IReadOnlyList<DraftHistoryItem> DraftHistory { get; init; } = [];
  public IReadOnlyList<VetoHistoryItem> VetoHistory { get; init; } = [];
}
