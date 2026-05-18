using ScreenDrafts.Modules.Drafts.Features.Participants.Get;

namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed record ParticipantListItem
{
  public required string PersonPublicId { get; init; }
  public string? DrafterPublicId { get; init; }
  public string? HostPublicId { get; init; }
  public required string DisplayName { get; init; }
  public bool IsCommissioner { get; init; }
  public bool IsRetired { get; init; }

  /// <summary>Synthesised roles: "Commissioner", "GM", "Host"</summary>
  public IReadOnlyList<string> Roles { get; init; } = [];

  // GM stats — null when person has no drafter profile
  public int? TotalDrafts { get; init; }
  public int? FilmsDrafted { get; init; }
  public int? VetoesUsed { get; init; }

  // Host stats — null when person has no host profile
  public int? DraftsHosted { get; init; }

  public string? ProfilePicturePath { get; init; }

  /// <summary>
  /// Null when the drafter has fewer than 5 canonical appearaces.
  /// </summary>
  public HonorificResponse? Honorific { get; init; }
}
