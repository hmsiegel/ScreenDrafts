namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

internal sealed class CreateSeriesCommandHandler(
  ISeriesRepository seriesRepository,
  IPublicIdGenerator publicIdGenerator)
    : ICommandHandler<CreateSeriesCommand, Guid>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<Guid>> Handle(CreateSeriesCommand CreateSeriesFeatureCommand, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Series);
    var kind = SeriesKind.FromValue(CreateSeriesFeatureCommand.Kind);
    var canonicalPolicy = CanonicalPolicy.FromValue(CreateSeriesFeatureCommand.CanonicalPolicy);
    var continuityScope = ContinuityScope.FromValue(CreateSeriesFeatureCommand.ContinuityScope);
    var continuityDateRule = ContinuityDateRule.FromValue(CreateSeriesFeatureCommand.ContinuityDateRule);

    var allowed = (DraftTypeMask)CreateSeriesFeatureCommand.AllowedDraftTypes;

    DraftType? defaultDraftType = null;

    if (CreateSeriesFeatureCommand.DefaultDraftType.HasValue)
    {
      defaultDraftType = DraftType.FromValue(CreateSeriesFeatureCommand.DefaultDraftType.Value);
    }

    var seriesResult = Series.Create(
      name: CreateSeriesFeatureCommand.Name,
      publicId: publicId,
      kind: kind,
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      allowedDraftTypes: allowed,
      defaultDraftType: defaultDraftType);

    if (seriesResult.IsFailure)
    {
      return Result.Failure<Guid>(seriesResult.Error!);
    }

    var series = seriesResult.Value;

    _seriesRepository.Add(series);

    return Result.Success(series.Id.Value);
  }
}




