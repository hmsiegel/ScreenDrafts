using ScreenDrafts.Common.Abstractions.Exceptions;
using ScreenDrafts.Modules.Drafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.Drafts.Domain.Participants;

/// <summary>
/// A participant can be a drafter or a team.
/// </summary>
/// <param name="Value"></param>
/// <param name="Kind"></param>
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

  public static Participant Community() => CommunityParticipants.PatreonMembers;

  public bool IsDrafter => Kind == ParticipantKind.Drafter;
  public bool IsTeam => Kind == ParticipantKind.Team;

  public bool IsCommunity => Kind == ParticipantKind.Community;

  public bool HasNoValue => Value == Guid.Empty;

  public DrafterId AsDrafterId() => !IsDrafter 
    ? throw new ScreenDraftsException("Participant is not a Drafter.") 
    : DrafterId.Create(Value);

  public DrafterTeamId AsDrafterTeamId() => !IsTeam
    ? throw new ScreenDraftsException("Participant is not a Team.")
    : DrafterTeamId.Create(Value);

  public Result Validate()
  {
    if (Value == Guid.Empty)
    {
      return Result.Failure(ParticipantErrors.EmptyValue);
    }

    var participantKind = Kind;

    if (!ParticipantKind.List.Any(k => k == participantKind))
    {
      return Result.Failure(ParticipantErrors.InvalidParticipantKind);
    }

    if (Kind == ParticipantKind.Community && !CommunityParticipants.IsKnownCommunity(this))
    {
      return Result.Failure(ParticipantErrors.UnknownCommunityParticipant);
    }

    if (Kind != ParticipantKind.Community && CommunityParticipants.IsCommunityId(Value))
    {
      return Result.Failure(ParticipantErrors.CommunityParticipantIdMismatch);
    }

    return Result.Success();
  }
}
