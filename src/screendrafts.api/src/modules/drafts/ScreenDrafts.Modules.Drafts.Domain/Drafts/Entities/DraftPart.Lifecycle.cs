namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
public sealed partial class DraftPart
{
  public Result StartDraft()
  {
    if (DraftStatus != DraftStatus.Created)
    {
      return Result.Failure(DraftErrors.DraftCanOnlyBeStartedIfItIsCreated);
    }

    if (_drafters.Count != TotalDrafters)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllDrafters);
    }

    if (PrimaryHost is null)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllHosts);
    }

    if (_draftHosts.Count != TotalHosts)
    {
      return Result.Failure(DraftErrors.CannotStartDraftWithoutAllHosts);
    }

    DraftStatus = DraftStatus.InProgress;

    Raise(new DraftStartedDomainEvent(Id.Value));

    return Result.Success();
  }

  public Result CompleteDraft()
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftIfItIsNotInProgress);
    }

    if (_picks.Count != TotalPicks)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftWithoutAllPicks);
    }

    DraftStatus = DraftStatus.Completed;

    Raise(new DraftCompletedDomainEvent(Id.Value));

    return Result.Success();
  }

  public Result PauseDraft()
  {
    if (DraftStatus != DraftStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotPauseDraftIfItIsNotInProgress);
    }
    DraftStatus = DraftStatus.Paused;
    Raise(new DraftPausedDomainEvent(Id.Value));
    return Result.Success();
  }

  public Result ContinueDraft()
  {
    if (DraftStatus != DraftStatus.Paused)
    {
      return Result.Failure(DraftErrors.CannotContinueDraftIfItIsNotPaused);
    }
    DraftStatus = DraftStatus.InProgress;
    Raise(new DraftContinuedDomainEvent(Id.Value));
    return Result.Success();
  }

  public void SetDraftStatus(DraftStatus draftStatus)
  {
    DraftStatus = draftStatus;
  }

}
