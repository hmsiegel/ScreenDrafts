namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddParticipantToDraftPart;

internal sealed class AddParticipantToDraftPartCommandHandler(
  IDraftPartRepository draftPartRepository,
  IDrafterRepository drafterRepository,
  IDrafterTeamRepository teamRepository)
  : ICommandHandler<AddParticipantToDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IDrafterTeamRepository _teamRepository = teamRepository;

  public async Task<Result> Handle(AddParticipantToDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByIdAsync(DraftPartId.Create(request.DraftPartId), cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    Participant participant;

    if (request.ParticipantKind == ParticipantKind.Community)
    {
      participant = CommunityParticipants.PatreonMembers;
    }
    else if (request.ParticipantKind == ParticipantKind.Drafter)
    {
      if (string.IsNullOrWhiteSpace(request.ParticipantPublicId))
      {
        return Result.Failure(DraftPartErrors.ParticpantPublicIdRequired);
      }

      var drafter = await _drafterRepository.GetByPublicIdAsync(request.ParticipantPublicId, cancellationToken);

      if (drafter is null)
      {
        return Result.Failure(DrafterErrors.NotFound(request.ParticipantPublicId));
      }

      participant = Participant.From(drafter.Id);
    }
    else if (request.ParticipantKind == ParticipantKind.Team)
    {
      if (string.IsNullOrWhiteSpace(request.ParticipantPublicId))
      {
        return Result.Failure(DraftPartErrors.ParticpantPublicIdRequired);
      }
      var team = await _teamRepository.GetByPublicIdAsync(request.ParticipantPublicId, cancellationToken);

      if (team is null)
      {
        return Result.Failure(DrafterTeamErrors.NotFound(request.ParticipantPublicId));
      }

      participant = Participant.From(team.Id);
    }
    else
    {
      return Result.Failure(DraftPartErrors.InvalidParticipantKind);
    }

    participant.Validate();

    var result = draftPart.AddParticipant(participant);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return result;
  }
}

