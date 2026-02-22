namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed class UpdateDraftCommandHandler(
  IDraftRepository draftsRepository,
  ISeriesRepository seriesRepository,
  ICampaignRepository campaignsRepository,
  ICategoryRepository categoriesRepository)
  : ICommandHandler<UpdateDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly ICampaignRepository _campaignsRepository = campaignsRepository;
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(UpdateDraftCommand UpdateDraftRequest, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetDraftByPublicId(UpdateDraftRequest.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(UpdateDraftRequest.PublicId));
    }

    if (draft.DraftStatus == DraftStatus.Completed || draft.DraftStatus == DraftStatus.Cancelled)
    {
      return Result.Failure(DraftErrors.CannotUpdateCompletedOrCancelledDraft(UpdateDraftRequest.PublicId));
    }

    var anyStartedPart = draft.Parts.Any(p => p.Status != DraftPartStatus.Created);
    if (anyStartedPart)
    {
      if (!string.Equals(draft.Series.PublicId, UpdateDraftRequest.SeriesPublicId, StringComparison.Ordinal))
      {
        return Result.Failure(DraftErrors.CannotChangeASeriesAfterADraftPartHasStarted);
      }

      if (draft.DraftType.Value != UpdateDraftRequest.DraftTypeValue)
      {
        return Result.Failure(DraftErrors.CannotChangeDraftTypeAfterADraftPartHasStarted);
      }
    }

    if (!string.IsNullOrEmpty(UpdateDraftRequest.SeriesPublicId))
    {
      if (!await _seriesRepository.ExistsByPublicIdAsync(UpdateDraftRequest.SeriesPublicId, cancellationToken))
      {
        return Result.Failure(SeriesErrors.SeriesIdIsInvalid(UpdateDraftRequest.SeriesPublicId));
      }

      var series = await _seriesRepository.GetByPublicIdAsync(UpdateDraftRequest.SeriesPublicId, cancellationToken);

      if (series is null)
      {
        return Result.Failure(SeriesErrors.NotFound(UpdateDraftRequest.SeriesPublicId));
      }

      draft.LinkSeries(series);
    }

    if (!string.IsNullOrEmpty(UpdateDraftRequest.CampaignPublicId))
    {
      if (!await _campaignsRepository.ExistsByPublicIdAsync(UpdateDraftRequest.CampaignPublicId, cancellationToken))
      {
        return Result.Failure(CampaignErrors.CampaignIdIsInvalid(UpdateDraftRequest.CampaignPublicId));
      }

      var campaign = await _campaignsRepository.GetByPublicIdAsync(UpdateDraftRequest.CampaignPublicId, cancellationToken);

      if (campaign is null)
      {
        return Result.Failure(CampaignErrors.NotFound(UpdateDraftRequest.CampaignPublicId));
      }

      draft.SetCampaign(campaign);
    }

    if (UpdateDraftRequest.PublicCategoryIds is { Count: > 0 })
    {
      var allExist = await _categoriesRepository.AllExistByPublicIdsAsync(UpdateDraftRequest.PublicCategoryIds, cancellationToken);
      if (!allExist)
      {
        return Result.Failure(CategoryErrors.OneOrMoreCategoryIdsAreInvalid(UpdateDraftRequest.PublicCategoryIds));
      }

      var categories = await _categoriesRepository.GetByPublicIdsAsync(UpdateDraftRequest.PublicCategoryIds, cancellationToken);

      if (categories.Count != UpdateDraftRequest.PublicCategoryIds.Count)
      {
        return Result.Failure(CategoryErrors.OneOrMoreCategoryIdsAreInvalid(UpdateDraftRequest.PublicCategoryIds));
      }

      draft.ReplaceCategories(categories);
    }

    draft.Update(
      title: UpdateDraftRequest.Title,
      description: UpdateDraftRequest.Description,
      draftTypeValue: UpdateDraftRequest.DraftTypeValue);

    _draftsRepository.Update(draft);

    return Result.Success();

  }
}



