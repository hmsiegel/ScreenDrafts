namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.GamePlay.GetGuestDraftGamePlay;

internal sealed record GetGuestDraftGameplayResponse
{
  public string GuestDraftPublicId { get; init; } = default!;
  public string Title { get; init; } = default!;
  public string Type { get; init; } = default!;
  public string Status { get; init; } = default!;

  /// <summary>Only populated when CallerContext.IsOwner -- the share token grants
  /// read access to anyone holding it, so it isn't exposed to non-owner
  /// participants even though they can see everything else here.</summary>
  public string? ShareToken { get; init; }

  public CallerContextResponse CallerContext { get; init; } = new();
  public IReadOnlyList<GuestDraftGameplayPositionResponse> Positions { get; init; } = [];
  public IReadOnlyList<GuestDraftGameplayParticipantResponse> Participants { get; init; } = [];
  public IReadOnlyList<GuestDraftGameplayPickResponse> Picks { get; init; } = [];
}
