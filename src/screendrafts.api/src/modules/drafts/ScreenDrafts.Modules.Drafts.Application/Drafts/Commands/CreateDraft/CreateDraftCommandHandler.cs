namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.CreateDraft;

internal sealed class CreateDraftCommandHandler(IDraftsRepository draftsRepository)
  : ICommandHandler<CreateDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;

  public async Task<Result<Guid>> Handle(CreateDraftCommand request, CancellationToken cancellationToken)
  {
    Series? series = null;

    if (request.SeriesId != Guid.Empty)
    {
      var seriesId = SeriesId.Create(request.SeriesId);

      series = await _draftsRepository.GetSeriesByIdAsync(seriesId, cancellationToken);

      if (series is null)
      {
        return Result.Failure<Guid>(DraftErrors.SeriesNotFound(request.SeriesId));
      }
    }

    var effectiveDraftType = series?.DefaultDraftType ?? request.DraftType ?? DraftType.Standard;

    var result = Draft.Create(
      new Title(request.Title),
      effectiveDraftType);

    if (result.IsFailure)
    {
      return await Task.FromResult(Result.Failure<Guid>(result.Error!));
    }

    var draft = result.Value;

    if (series is not null)
    {
      draft.LinkSeries(series);
    }

    if (request.AutoCreateFirstPart)
    {
      var partResult = draft.AddPart(
        1,
        request.TotalPicks,
        request.TotalDrafters,
        request.TotalDrafterTeams,
        request.TotalHosts);

      if (partResult.IsFailure)
      {
        return Result.Failure<Guid>(partResult.Error!);
      }
    }

    _draftsRepository.Add(draft);
    return draft.Id.Value;
  }
}
