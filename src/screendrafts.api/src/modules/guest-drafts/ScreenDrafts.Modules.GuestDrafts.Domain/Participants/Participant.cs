using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.GuestDrafts.Domain.Participants;

/// <summary>
/// A participant can be a guest drafter or a guest drafter team -- mirrors
/// canonical Participant exactly, minus Community (never existed here).
/// </summary>
public readonly record struct Participant(Guid Value, ParticipantKind Kind)
{
  public static Participant From(DrafterId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, ParticipantKind.Drafter);
  }

  public static Participant From(DrafterTeamId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, ParticipantKind.Team);
  }

  public bool IsDrafter => Kind == ParticipantKind.Drafter;
  public bool IsTeam => Kind == ParticipantKind.Team;
  public bool HasNoValue => Value == Guid.Empty;

  public DrafterId AsGuestDrafterId() =>
    !IsDrafter
      ? throw new ScreenDraftsException("Participant is not a GuestDrafter.")
      : DrafterId.Create(Value);

  public DrafterTeamId AsGuestDrafterTeamId() =>
    !IsTeam
      ? throw new ScreenDraftsException("Participant is not a GuestDrafterTeam.")
      : DrafterTeamId.Create(Value);

  public Result Validate()
  {
    if (Value == Guid.Empty)
    {
      return Result.Failure(ParticipantErrors.EmptyValue);
    }

    var kind = Kind; // copy instance member to local to avoid capturing 'this' in the lambda
    if (!ParticipantKind.List.Any(k => k == kind))
    {
      return Result.Failure(ParticipantErrors.InvalidParticipantKind);
    }

    return Result.Success();
  }
}
