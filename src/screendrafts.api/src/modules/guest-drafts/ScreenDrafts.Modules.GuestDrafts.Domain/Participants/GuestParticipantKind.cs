namespace ScreenDrafts.Modules.GuestDrafts.Domain.Participants;

public sealed class GuestParticipantKind(string name, int value)
  : SmartEnum<GuestParticipantKind>(name, value)
{
  public static readonly GuestParticipantKind Drafter = new(nameof(Drafter), 0);
  public static readonly GuestParticipantKind Team = new(nameof(Team), 1);
}
