namespace ScreenDrafts.Modules.Drafts.Features.Series.Create;

internal sealed class CommandHandler(
  ISeriesRepository seriesRepository,
  IPublicIdGenerator publicIdGenerator)
    : Common.Features.Abstractions.Messaging.ICommandHandler<Command, Guid>
{
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<Guid>> Handle(Command command, CancellationToken cancellationToken)
  {
    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Series);
    var kind = SeriesKind.FromValue(command.Kind);
    var canonicalPolicy = CanonicalPolicy.FromValue(command.CanonicalPolicy);
    var continuityScope = ContinuityScope.FromValue(command.ContinuityScope);
    var continuityDateRule = ContinuityDateRule.FromValue(command.ContinuityDateRule);

    var allowed = (DraftTypeMask)command.AllowedDraftTypes;

    DraftType? defaultDraftType = null;

    if (command.DefaultDraftType.HasValue)
    {
      defaultDraftType = DraftType.FromValue(command.DefaultDraftType.Value);
    }

    var seriesResult = Domain.Drafts.Entities.Series.Create(
      name: command.Name,
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


