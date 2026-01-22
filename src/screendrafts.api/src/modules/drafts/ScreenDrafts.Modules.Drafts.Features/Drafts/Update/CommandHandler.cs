namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed class CommandHandler(
  IDraftsRepository draftsRepository,
  ISeriesRepository seriesRepository,
  ICampaignsRepository campaignsRepository,
  ICategoriesRepository categoriesRepository)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly ICampaignsRepository _campaignsRepository = campaignsRepository;
  private readonly ICategoriesRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    if (draft.DraftStatus == DraftStatus.Completed || draft.DraftStatus == DraftStatus.Cancelled)
    {
      return Result.Failure(DraftErrors.CannotUpdateCompletedOrCancelledDraft(request.PublicId));
    }

    var anyStartedPart = draft.Parts.Any(p => p.Status != DraftPartStatus.Created);
    if (anyStartedPart)
    {
      if (!string.Equals(draft.Series.PublicId, request.SeriesPublicId, StringComparison.Ordinal))
      {
        return Result.Failure(DraftErrors.CannotChangeASeriesAfterADraftPartHasStarted);
      }

      if (draft.DraftType.Value != request.DraftTypeValue)
      {
        return Result.Failure(DraftErrors.CannotChangeDraftTypeAfterADraftPartHasStarted);
      }
    }

    if (!await _seriesRepository.ExistsByPublicIdAsync(request.SeriesPublicId, cancellationToken))
    {
      return Result.Failure(SeriesErrors.SeriesIdIsInvalid(request.SeriesPublicId));
    }

    if (!await _campaignsRepository.ExistsByPublicIdAsync(request.CampaignPublicId, cancellationToken))
    {
      return Result.Failure(CampaignErrors.CampaignIdIsInvalid(request.CampaignPublicId));
    }

    if (request.PublicCategoryIds is {  Count: > 0 })
    {
      var allExist = await _categoriesRepository.AllExistByPublicIdsAsync(request.PublicCategoryIds, cancellationToken);
      if (!allExist)
      {
        return Result.Failure(CategoryErrors.OneOrMoreCategoryIdsAreInvalid(request.PublicCategoryIds));
      }
    }

    var series = await _seriesRepository.GetByPublicIdAsync(request.SeriesPublicId!, cancellationToken);

    if (series is null)
    {
      return Result.Failure(SeriesErrors.NotFound(request.SeriesPublicId!));
    }

    draft.LinkSeries(series);

    var campaign = await _campaignsRepository.GetByPublicIdAsync(request.CampaignPublicId!, cancellationToken);

    if (campaign is null)
    {
      return Result.Failure(CampaignErrors.NotFound(request.CampaignPublicId!));
    }

    draft.SetCampaign(campaign);

    draft.Update(
      title: request.Title,
      description: request.Description,
      draftTypeValue: request.DraftTypeValue);

    var categories = await _categoriesRepository.GetByPublicIdsAsync(request.PublicCategoryIds ?? [], cancellationToken);

    if (categories.Count != request.PublicCategoryIds!.Count)
    {
      return Result.Failure(CategoryErrors.OneOrMoreCategoryIdsAreInvalid(request.PublicCategoryIds ?? []));
    }

    draft.ReplaceCategories(categories);

    _draftsRepository.Update(draft);

    return Result.Success();

  }
}
