namespace ScreenDrafts.Modules.Drafts.Features.Series.Edit;

internal sealed class CommandHandler(ISeriesRepository seriesRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var series = await _seriesRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.NotFound(request.PublicId));
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
