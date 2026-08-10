namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Seed.SeedRevealPick;

internal sealed class SeedRevealPickCommandHandler(
  IDraftPartRepository draftPartRepository,
  ISeriesPolicyProvider seriesPolicyProvider
) : ICommandHandler<SeedRevealPickCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ISeriesPolicyProvider _seriesPolicyProvider = seriesPolicyProvider;

  public async Task<Result> Handle(
    SeedRevealPickCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var series = await _seriesPolicyProvider.GetSeriesAsyc(draftPart.SeriesId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(draftPart.SeriesId.Value));
    }

    var result = draftPart.RevealPick(
      playOrder: request.PlayOrder,
      actedByPublicId: request.ActedByPublicId,
      canonicalPolicyValue: CanonicalPolicy.FromValue(series.CanonicalPolicy.Value)
    );

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
