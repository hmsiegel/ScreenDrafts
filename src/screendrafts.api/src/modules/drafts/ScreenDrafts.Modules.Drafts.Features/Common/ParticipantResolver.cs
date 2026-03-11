namespace ScreenDrafts.Modules.Drafts.Features.Common;

public sealed class ParticipantResolver(
  IDrafterRepository repository,
  IDrafterTeamRepository drafterTeamRepository)
{
  private readonly IDrafterRepository _drafterRepository = repository;
  private readonly IDrafterTeamRepository _drafterTeamRepository = drafterTeamRepository;

  public async Task<Result<Participant>> ResolveAsync(string? participantPublicId, ParticipantKind participantKind, CancellationToken ct)
  {
    if (participantKind == ParticipantKind.Community)
    {
      return CommunityParticipants.PatreonMembers;
    }

    if (string.IsNullOrEmpty(participantPublicId))
    {
      return Result.Failure<Participant>(DraftPartErrors.ParticpantPublicIdRequired);
    }

    if (participantKind == ParticipantKind.Drafter)
    {
      var drafter = await _drafterRepository.GetByPublicIdAsync(participantPublicId, ct);

      if (drafter is null)
      {
        return Result.Failure<Participant>(DrafterErrors.NotFound(participantPublicId));
      }

      return Participant.From(drafter.Id);

    }
    if (participantKind == ParticipantKind.Team)
    {
      var drafterTeam = await _drafterTeamRepository.GetByPublicIdAsync(participantPublicId, ct);
      if (drafterTeam is null)
      {
        return Result.Failure<Participant>(DrafterTeamErrors.NotFound(participantPublicId));
      }
      return Participant.From(drafterTeam.Id);
    }
    
    return Result.Failure<Participant>(DraftPartErrors.InvalidParticipantKind);
  }

  public async Task<Participant?> ResolveByParticpantIdAsync(Guid participantId, ParticipantKind participantKind, CancellationToken cancellationToken)
  {
    if (participantKind == ParticipantKind.Community)
    {
      return CommunityParticipants.PatreonMembers;
    }

    if (participantKind == ParticipantKind.Drafter)
    {
      var drafter = await _drafterRepository.GetByIdAsync(DrafterId.Create(participantId), cancellationToken);

      if (drafter is null)
      {
        return null;
      }

      return Participant.From(drafter.Id);
    }

    if (participantKind == ParticipantKind.Team)
    {
      var team = await _drafterTeamRepository.GetByIdAsync(DrafterTeamId.Create(participantId), cancellationToken);

      if (team is null)
      {
        return null; 
      }

      return Participant.From(team.Id);
    }
    
    return null;
  }
}
