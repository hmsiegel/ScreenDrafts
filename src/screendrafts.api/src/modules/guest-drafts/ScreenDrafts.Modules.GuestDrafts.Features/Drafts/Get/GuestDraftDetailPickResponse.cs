namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Get;

internal sealed record GuestDraftDetailPickResponse
{
  public int Position { get; init; }
  public string? MoviePublicId { get; init; }
  public string? MovieTitle { get; init; }
  public string? MovieYear { get; init; }
  public int? TmdbId { get; init; }
  public string? PlayedByDisplayName { get; init; }
  public bool WasVetoed { get; init; }
  public bool WasVetoOverridden { get; init; }
  public bool WasCommissionerOverride { get; init; }
  public bool IsActiveOnFinalBoard { get; init; }
  public string? VetoedByDisplayName { get; init; }
  public string? SavedByDisplayName { get; init; }
}
