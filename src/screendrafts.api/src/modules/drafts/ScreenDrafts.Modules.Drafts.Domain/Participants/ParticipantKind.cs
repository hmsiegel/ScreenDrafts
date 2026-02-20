namespace ScreenDrafts.Modules.Drafts.Domain.Participants;

public sealed class ParticipantKind(string name, int value) : SmartEnum<ParticipantKind>(name, value)
{
  public static readonly ParticipantKind Drafter = new(nameof(Drafter), 0);
  public static readonly ParticipantKind Team = new(nameof(Team), 1);
  public static readonly ParticipantKind Community = new(nameof(Community), 2);
}
