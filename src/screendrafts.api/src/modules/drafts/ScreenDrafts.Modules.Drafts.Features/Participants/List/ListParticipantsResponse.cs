namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed record ListParticipantsResponse
{
  public PagedResult<ParticipantListItem>? Results { get; init; }
}
