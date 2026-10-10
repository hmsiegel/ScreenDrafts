namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record GetParticipantProfileQuery : IQuery<GetParticipantProfileResponse>
{
  public string PersonPublicId { get; init; } = default!;
  public bool IncludePatreon { get; init; }

  /// <summary>Release channel to report on: 0 = main feed, 1 = Patreon. One channel per request.</summary>
  public int Channel { get; init; }
}
