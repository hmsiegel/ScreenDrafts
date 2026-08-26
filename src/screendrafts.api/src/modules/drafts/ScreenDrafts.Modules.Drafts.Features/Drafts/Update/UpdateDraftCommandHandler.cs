namespace ScreenDrafts.Modules.Drafts.Features.Drafts.Update;

internal sealed class UpdateDraftCommandHandler(
  IDraftRepository draftsRepository,
  ISeriesRepository seriesRepository,
  ICampaignRepository campaignsRepository,
  ICategoryRepository categoriesRepository
) : ICommandHandler<UpdateDraftCommand>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly ISeriesRepository _seriesRepository = seriesRepository;
  private readonly ICampaignRepository _campaignsRepository = campaignsRepository;
  private readonly ICategoryRepository _categoriesRepository = categoriesRepository;

  public async Task<Result> Handle(UpdateDraftCommand request, CancellationToken cancellationToken)
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

      // FungibleTokenName drives ApplyRolloversAsync's whole grant decision the moment
      // Part 1 starts — changing it afterward would silently desync already-started parts
      // from whatever a later part computes, the same class of problem the Series/DraftType
      // guards above already exist to prevent.
      if (
        !string.IsNullOrWhiteSpace(request.FungibleTokenName)
        && !string.Equals(
          draft.FungibleTokenName,
          request.FungibleTokenName,
          StringComparison.Ordinal
        )
      )
      {
        return Result.Failure(DraftErrors.CannotChangeFungibleTokenNameAfterADraftPartHasStarted);
      }

      if (request.IsHostless.HasValue && request.IsHostless.Value != draft.IsHostless)
      {
        return Result.Failure(DraftErrors.CannotChangeIsHostlessAfterADraftPartHasStarted);
      }
    }

    if (!string.IsNullOrEmpty(request.SeriesPublicId))
    {
      if (!await _seriesRepository.ExistsByPublicIdAsync(request.SeriesPublicId, cancellationToken))
      {
        return Result.Failure(SeriesErrors.SeriesIdIsInvalid(request.SeriesPublicId));
      }

      var series = await _seriesRepository.GetByPublicIdAsync(
        request.SeriesPublicId,
        cancellationToken
      );

      if (series is null)
      {
        return Result.Failure(SeriesErrors.SeriesNotFound(request.SeriesPublicId));
      }

      draft.LinkSeries(series);
    }

    if (!string.IsNullOrEmpty(request.CampaignPublicId))
    {
      if (
        !await _campaignsRepository.ExistsByPublicIdAsync(
          request.CampaignPublicId,
          cancellationToken
        )
      )
      {
        return Result.Failure(CampaignErrors.CampaignIdIsInvalid(request.CampaignPublicId));
      }

      var campaign = await _campaignsRepository.GetByPublicIdAsync(
        request.CampaignPublicId,
        cancellationToken
      );

      if (campaign is null)
      {
        return Result.Failure(CampaignErrors.NotFound(request.CampaignPublicId));
      }

      draft.SetCampaign(campaign);
    }

    if (request.PublicCategoryIds is { Count: > 0 })
    {
      var allExist = await _categoriesRepository.AllExistByPublicIdsAsync(
        request.PublicCategoryIds,
        cancellationToken
      );
      if (!allExist)
      {
        return Result.Failure(
          CategoryErrors.OneOrMoreCategoryIdsAreInvalid(request.PublicCategoryIds)
        );
      }

      var categories = await _categoriesRepository.GetByPublicIdsAsync(
        request.PublicCategoryIds,
        cancellationToken
      );

      if (categories.Count != request.PublicCategoryIds.Count)
      {
        return Result.Failure(
          CategoryErrors.OneOrMoreCategoryIdsAreInvalid(request.PublicCategoryIds)
        );
      }

      draft.ReplaceCategories(categories);
    }

    // Only sets when a non-blank value is provided — matches the Series/Campaign pattern
    // above (apply only if present), not the always-applies Title/Description pattern
    // below. There's deliberately no way to clear FungibleTokenName back to null through
    // this endpoint yet; add a dedicated path if that turns out to be needed.
    if (!string.IsNullOrWhiteSpace(request.FungibleTokenName))
    {
      draft.SetFungibleTokenName(request.FungibleTokenName);
    }

    if (request.IsHostless.HasValue)
    {
      draft.SetIsHostless(request.IsHostless.Value);
    }

    draft.Update(
      title: request.Title,
      description: request.Description,
      draftTypeValue: request.DraftTypeValue
    );

    _draftsRepository.Update(draft);

    return Result.Success();
  }
}
