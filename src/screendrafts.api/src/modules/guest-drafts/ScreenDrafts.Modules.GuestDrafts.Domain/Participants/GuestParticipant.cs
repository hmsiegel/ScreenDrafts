namespace ScreenDrafts.Modules.GuestDrafts.Domain.Participants;

/// <summary>
/// A participant can be a guest drafter or a guest drafter team -- mirrors
/// canonical Participant exactly, minus Community (never existed here).
/// </summary>
public readonly record struct GuestParticipant(Guid Value, GuestParticipantKind Kind)
{
  public static GuestParticipant From(GuestDrafterId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, GuestParticipantKind.Drafter);
  }

  public static GuestParticipant From(GuestDrafterTeamId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    return new(id.Value, GuestParticipantKind.Team);
  }

  public bool IsDrafter => Kind == GuestParticipantKind.Drafter;
  public bool IsTeam => Kind == GuestParticipantKind.Team;
  public bool HasNoValue => Value == Guid.Empty;

  public GuestDrafterId AsGuestDrafterId() =>
    !IsDrafter
      ? throw new ScreenDraftsException("Participant is not a GuestDrafter.")
      : GuestDrafterId.Create(Value);

  public GuestDrafterTeamId AsGuestDrafterTeamId() =>
    !IsTeam
      ? throw new ScreenDraftsException("Participant is not a GuestDrafterTeam.")
      : GuestDrafterTeamId.Create(Value);

  public Result Validate()
  {
    if (Value == Guid.Empty)
    {
      return Result.Failure(GuestParticipantErrors.EmptyValue);
    }

    var kind = Kind; // copy instance member to local to avoid capturing 'this' in the lambda
    if (!GuestParticipantKind.List.Any(k => k == kind))
    {
      return Result.Failure(GuestParticipantErrors.InvalidParticipantKind);
    }

    return Result.Success();
  }
}
