namespace ScreenDrafts.Modules.Drafts.Features.Common;

public sealed class ParticipantResolver(
  IDrafterRepository repository,
  IDrafterTeamRepository drafterTeamRepository)
{
  private readonly IDrafterRepository _repository = repository;
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
      var drafter = await _repository.GetByPublicIdAsync(participantPublicId, ct);

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
}
