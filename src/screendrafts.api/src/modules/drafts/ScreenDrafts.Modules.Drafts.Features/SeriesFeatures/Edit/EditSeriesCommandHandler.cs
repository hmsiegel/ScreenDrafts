namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed class EditSeriesCommandHandler(ISeriesRepository seriesRepository)
  : ICommandHandler<EditSeriesCommand>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;

  public async Task<Result> Handle(EditSeriesCommand EditSeriesFeatureRequest, CancellationToken cancellationToken)
  {
    var series = await _seriesRepository.GetByPublicIdAsync(EditSeriesFeatureRequest.PublicId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.NotFound(EditSeriesFeatureRequest.PublicId));
    }

    var kind = SeriesKind.FromValue(EditSeriesFeatureRequest.Kind);
    var canonicalPolicy = CanonicalPolicy.FromValue(EditSeriesFeatureRequest.CanonicalPolicy);
    var continuityScope = ContinuityScope.FromValue(EditSeriesFeatureRequest.ContinuityScope);
    var continuityDateRule = ContinuityDateRule.FromValue(EditSeriesFeatureRequest.ContinuityDateRule);

    var allowedDraftTypes = (DraftTypeMask)EditSeriesFeatureRequest.AllowedDraftTypes;

    DraftType? defaultDraftType = null;
    if (EditSeriesFeatureRequest.DefaultDraftType.HasValue)
    {
      defaultDraftType = DraftType.FromValue(EditSeriesFeatureRequest.DefaultDraftType.Value);
    }

    var rename = series.Rename(EditSeriesFeatureRequest.Name);

    if (rename.IsFailure)
    {
      return rename;
    }

    var policies = series.UpdatePolicies(
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      kind: kind);

    if (policies.IsFailure)
    {
      return policies;
    }

    var format = series.UpdateFormatRules(
      allowedDraftTypes: allowedDraftTypes,
      defaultDraftType: defaultDraftType);

    if (format.IsFailure)
    {
      return format;
    }

    _seriesRepository.Update(series);

    return Result.Success();
  }
}



