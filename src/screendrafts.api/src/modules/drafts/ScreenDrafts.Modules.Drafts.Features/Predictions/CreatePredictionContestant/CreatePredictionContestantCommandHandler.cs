namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionContestant;

internal sealed class CreatePredictionContestantCommandHandler(
  IPersonRepository personRepository,
  IPredictionContestantRepository contestantRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreatePredictionContestantCommand, string>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IPredictionContestantRepository _contestantRepository = contestantRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  public async Task<Result<string>> Handle(
    CreatePredictionContestantCommand request,
    CancellationToken cancellationToken)
  {
    var person = await _personRepository.GetByPublicIdAsync(
      request.PersonPublicId,
      cancellationToken);

    if (person is null)
    {
      return Result.Failure<string>(PersonErrors.NotFound(request.PersonPublicId));
    }

    var existing = await _contestantRepository.GetByPersonIdAsync(
      person.Id,
      cancellationToken);

    if (existing is not null)
    {
      return Result.Failure<string>(
        PredictionErrors.ContestantAlreadyExists(request.PersonPublicId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.PredictionContestant);

    var contestant = PredictionContestant.Create(person, publicId);

    _contestantRepository.Add(contestant);

    return Result.Success(contestant.PublicId);
  }
}
