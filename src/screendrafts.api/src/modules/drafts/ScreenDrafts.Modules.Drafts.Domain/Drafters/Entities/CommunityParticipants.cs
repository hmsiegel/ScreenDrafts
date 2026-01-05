namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.Entities;

public static class CommunityParticipants
{
  public static readonly ParticipantId PatreonMembers = new(
    Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    ParticipantKind.Community);
}
