namespace ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraftStatus;

internal sealed class GetDraftStatusQueryHandler(
  IDraftRepository draftsRepository,
  IDateTimeProvider dateTimeProvider)
  : IQueryHandler<GetDraftStatusQuery, Response>
{
  private readonly IDraftRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<Response>> Handle(GetDraftStatusQuery GetDraftStatusRequest, CancellationToken cancellationToken)
  {
    var utcNow = _dateTimeProvider.UtcNow;

    var draft = await _draftsRepository.GetDraftByPublicIdWithPartsAsNoTrackingAsync(GetDraftStatusRequest.DraftPublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<Response>(DraftErrors.NotFound(GetDraftStatusRequest.DraftPublicId));
    }

    var draftView = draft.GetLifecycleView(utcNow);
    var actionPartIndex = ResolveActionPartIndex(draft, draftView, utcNow);
    var draftActions = ResolveDraftActions(draftView, actionPartIndex);

    var parts = draft.Parts
      .OrderBy(p => p.PartIndex)
      .Select(p =>
      {
        var partView = p.GetLifecycleView(utcNow);
        return new DraftPartStatusResponse
        {
          DraftPartId = p.Id.Value,
          PartIndex = p.PartIndex,
          Status = p.Status.Name,
          Lifecycleview = partView.ToString(),
          ScheduledForUtc = p.ScheduledForUtc,
          Actions = ResolvePartActions(partView)
        };
      })
      .ToList();

    return Result.Success(new Response
    {
      DraftPublicId = draft.PublicId,
      DraftStatus = draft.DraftStatus.Name,
      Lifecycleview = draftView.ToString(),
      Actions = draftActions,
      ActionPartIndex = actionPartIndex,
      Parts = parts
    });
  }

  private static int? ResolveActionPartIndex(Draft draft, DraftLifecycleView draftView, DateTime utcNow)
  {
    return draftView switch
    {
      DraftLifecycleView.Paused =>
        draft.Parts
          .Where(p => p.IsScheduled(utcNow))
          .OrderBy(p => p.PartIndex)
          .Select(p => (int?)p.PartIndex)
          .FirstOrDefault(),

      DraftLifecycleView.Scheduled or DraftLifecycleView.Created =>
        draft.Parts
          .OrderBy(p => p.PartIndex)
          .OrderByDescending(p => p.IsScheduled(utcNow))
          .Select(p => (int?)p.PartIndex)
          .FirstOrDefault(),

      _ => null
    };
  }

  private static string[] ResolveDraftActions(
    DraftLifecycleView view,
    int? actionPartIndex)
  {
    if (actionPartIndex is null)
    {
      return [];
    }

    return view switch
    {
      DraftLifecycleView.Created or DraftLifecycleView.Scheduled
        => ["Start"],

      DraftLifecycleView.Paused
       => ["Continue"],

      _ => []
    };
  }

  private static string[] ResolvePartActions(
    DraftPartLifecycleView view)
  {
    return view switch
    {
      DraftPartLifecycleView.Created or DraftPartLifecycleView.Scheduled
        => ["Start"],

      DraftPartLifecycleView.InProgress
       => ["Complete"],

      _ => []
    };
  }
}



