namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

internal sealed class CreateSeriesCommandHandler(
  ISeriesRepository seriesRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateSeriesCommand, Guid>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<Guid>> Handle(
    CreateSeriesCommand request,
    CancellationToken cancellationToken
  )
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Series);
    var kind = SeriesKind.FromValue(request.Kind);
    var canonicalPolicy = CanonicalPolicy.FromValue(request.CanonicalPolicy);
    var continuityScope = ContinuityScope.FromValue(request.ContinuityScope);
    var continuityDateRule = ContinuityDateRule.FromValue(request.ContinuityDateRule);

    var allowed = (DraftTypeMask)request.AllowedDraftTypes;

    DraftType? defaultDraftType = null;

    if (request.DefaultDraftType.HasValue)
    {
      defaultDraftType = DraftType.FromValue(request.DefaultDraftType.Value);
    }

    var seriesResult = Series.Create(
      name: request.Name,
      description: request.Description,
      publicId: publicId,
      kind: kind,
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      allowedDraftTypes: allowed,
      defaultDraftType: defaultDraftType
    );

    if (seriesResult.IsFailure)
    {
      return Result.Failure<Guid>(seriesResult.Error!);
    }

    var series = seriesResult.Value;

    _seriesRepository.Add(series);

    return Result.Success(series.Id.Value);
  }
}
