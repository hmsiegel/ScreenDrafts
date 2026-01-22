namespace ScreenDrafts.Modules.Drafts.Domain.Drafters.ValueObjects;

/// <summary>
/// A participant can be a drafter or a team.
/// </summary>
/// <param name="Value"></param>
/// <param name="Kind"></param>
public readonly record struct ParticipantId(Guid Value, ParticipantKind Kind)
{
  public static ParticipantId From(DrafterId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, ParticipantKind.Drafter);
  }

  public static ParticipantId From(DrafterTeamId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, ParticipantKind.Team);
  }

  public bool IsDrafter => Kind == ParticipantKind.Drafter;
  public bool IsTeam => Kind == ParticipantKind.Team;

  public bool HasNoValue => Value == Guid.Empty;

  public DrafterId AsDrafterId() => !IsDrafter 
    ? throw new InvalidOperationException("Participant is not a Drafter.") 
    : DrafterId.Create(Value);

  public DrafterTeamId AsDrafterTeamId() => !IsTeam
    ? throw new InvalidOperationException("Participant is not a Team.")
    : DrafterTeamId.Create(Value);
}
