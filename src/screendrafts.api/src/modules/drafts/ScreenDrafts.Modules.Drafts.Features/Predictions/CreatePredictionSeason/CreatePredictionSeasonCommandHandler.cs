namespace ScreenDrafts.Modules.Drafts.Features.Predictions.CreatePredictionSeason;

internal sealed class CreatePredictionSeasonCommandHandler(
  IPredictionSeasonRepository repository,
  IPublicIdGenerator publicIdGenerator) : ICommandHandler<CreatePredictionSeasonCommand, string>
{
  private readonly IPredictionSeasonRepository _predictionSeasonRepository = repository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreatePredictionSeasonCommand request, CancellationToken cancellationToken)
  {

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.PredictionSeason);

    var season = PredictionSeason.Create(
      request.Number,
      request.StartsOn,
      publicId);

    _predictionSeasonRepository.Add(season);

    return Result.Success(publicId);
  }
}
