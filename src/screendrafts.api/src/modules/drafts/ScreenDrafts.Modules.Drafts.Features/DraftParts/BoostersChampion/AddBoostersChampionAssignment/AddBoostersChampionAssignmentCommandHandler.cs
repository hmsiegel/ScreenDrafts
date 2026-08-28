namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.BoostersChampion.AddBoostersChampionAssignment;

internal sealed class AddBoostersChampionAssignmentCommandHandler(
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<AddBoostersChampionAssignmentCommand, string>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    AddBoostersChampionAssignmentCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure<string>(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var participantResult = await _participantResolver.ResolveAsync(
      request.AssignedDrafterPublicId,
      ParticipantKind.Drafter,
      cancellationToken
    );

    if (participantResult.IsFailure)
    {
      return Result.Failure<string>(participantResult.Errors);
    }

    var participant = participantResult.Value;

    var validationResult = participant.Validate();

    if (validationResult.IsFailure)
    {
      return Result.Failure<string>(validationResult.Errors);
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.BoostersChampionAssignment);

    var result = draftPart.AddBoostersChampionAssignment(
      publicId,
      participant.Value,
      request.TmdbId
    );

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success(publicId);
  }
}
