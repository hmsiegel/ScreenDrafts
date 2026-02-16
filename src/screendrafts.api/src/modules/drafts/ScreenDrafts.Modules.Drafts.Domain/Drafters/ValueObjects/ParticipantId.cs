using ScreenDrafts.Common.Abstractions.Exceptions;

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

  public static ParticipantId Community() => CommunityParticipants.PatreonMembers;

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

  public void Validate()
  {
    if (Value == Guid.Empty)
    {
      throw new ScreenDraftsException("ParticipantId value cannot be empty.");
    }

    var participantKind = Kind;

    if (!ParticipantKind.List.Any(k => k == participantKind))
    {
      throw new ScreenDraftsException("Invalid ParticipantKind.");
    }

    if (Kind == ParticipantKind.Community && !CommunityParticipants.IsKnownCommunity(this))
    {
      throw new ScreenDraftsException("Unknown community participant.");
    }

    if (Kind != ParticipantKind.Community && CommunityParticipants.IsCommunityId(Value))
    {
      throw new ScreenDraftsException("Community participant ID does not match participant kind.");
    }
  }
}
