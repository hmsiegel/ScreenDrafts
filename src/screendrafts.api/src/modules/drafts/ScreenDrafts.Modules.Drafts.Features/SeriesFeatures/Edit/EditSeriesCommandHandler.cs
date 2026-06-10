namespace ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Edit;

internal sealed class EditSeriesCommandHandler(ISeriesRepository seriesRepository)
  : ICommandHandler<EditSeriesCommand>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;

  public async Task<Result> Handle(EditSeriesCommand request, CancellationToken cancellationToken)
  {
    var series = await _seriesRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.SeriesNotFound(request.PublicId));
    }

    var kind = SeriesKind.FromValue(request.Kind);
    var canonicalPolicy = CanonicalPolicy.FromValue(request.CanonicalPolicy);
    var continuityScope = ContinuityScope.FromValue(request.ContinuityScope);
    var continuityDateRule = ContinuityDateRule.FromValue(request.ContinuityDateRule);

    var allowedDraftTypes = (DraftTypeMask)request.AllowedDraftTypes;

    DraftType? defaultDraftType = null;
    if (request.DefaultDraftType.HasValue)
    {
      defaultDraftType = DraftType.FromValue(request.DefaultDraftType.Value);
    }

    var rename = series.Rename(request.Name);

    if (rename.IsFailure)
    {
      return rename;
    }

    series.UpdateDescription(request.Description);

    var policies = series.UpdatePolicies(
      canonicalPolicy: canonicalPolicy,
      continuityScope: continuityScope,
      continuityDateRule: continuityDateRule,
      kind: kind
    );

    if (policies.IsFailure)
    {
      return policies;
    }

    var format = series.UpdateFormatRules(
      allowedDraftTypes: allowedDraftTypes,
      defaultDraftType: defaultDraftType
    );

    if (format.IsFailure)
    {
      return format;
    }

    _seriesRepository.Update(series);

    return Result.Success();
  }
}
