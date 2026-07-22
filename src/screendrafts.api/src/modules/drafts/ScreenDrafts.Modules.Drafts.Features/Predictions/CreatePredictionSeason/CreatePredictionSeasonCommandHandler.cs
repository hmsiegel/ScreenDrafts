namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed class CreatePredictionSeasonCommandHandler(
  IPredictionSeasonRepository repository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreatePredictionSeasonCommand, string>
{
  private readonly IPredictionSeasonRepository _predictionSeasonRepository = repository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreatePredictionSeasonCommand request,
    CancellationToken cancellationToken
  )
  {
    var numberExists = await _predictionSeasonRepository.ExistsByNumberAsync(
      request.Number,
      cancellationToken
    );

    if (numberExists)
    {
      return Result.Failure<string>(PredictionErrors.SeasonNumberAlreadyExists(request.Number));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.PredictionSeason);

    var seasonResult = PredictionSeason.Create(request.Number, request.StartsOn, publicId);

    if (seasonResult.IsFailure)
    {
      return Result.Failure<string>(seasonResult.Errors[0]);
    }

    _predictionSeasonRepository.Add(seasonResult.Value);

    return Result.Success(publicId);
  }
}
