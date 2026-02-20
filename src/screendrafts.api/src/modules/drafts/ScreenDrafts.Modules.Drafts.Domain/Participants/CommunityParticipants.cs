namespace ScreenDrafts.Modules.Drafts.Domain.Participants;

public static class CommunityParticipants
{
  public static readonly Participant PatreonMembers = new(
    Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
    ParticipantKind.Community);

  private static readonly HashSet<Guid> _knownCommunityIds =
  new HashSet<Guid>
  {
    PatreonMembers.Value
  };

  public static bool IsCommunityId(Guid value) =>
    _knownCommunityIds.Contains(value);


  public static bool IsKnownCommunity(Participant participantId) =>
    participantId.Kind == ParticipantKind.Community &&
    _knownCommunityIds.Contains(participantId.Value);
}
