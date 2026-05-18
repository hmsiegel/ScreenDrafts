namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed record GetParticipantProfileQuery : IQuery<GetParticipantProfileResponse>
{
  public string PersonPublicId { get; init; } = default!;
  public bool IncludePatreon { get; init; }
}
