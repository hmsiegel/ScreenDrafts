namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
public sealed partial class DraftPart
{
  internal Result Start()
  {
    if (Status != DraftPartStatus.Created)
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

    Status = DraftPartStatus.InProgress;

    Raise(new DraftPartStartedDomainEvent(
      Id.Value,
      Draft.Id.Value,
      PartIndex));

    return Result.Success();
  }

  internal Result Complete()
  {
    if (Status != DraftPartStatus.InProgress)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftIfItIsNotInProgress);
    }

    if (_picks.Count != TotalPicks)
    {
      return Result.Failure(DraftErrors.CannotCompleteDraftWithoutAllPicks);
    }

    Status = DraftPartStatus.Completed;
    UpdatedAtUtc = DateTime.UtcNow;


    return Result.Success();
  }

  public void SetDraftStatus(DraftPartStatus draftStatus)
  {
    Status = draftStatus;
  }
}
