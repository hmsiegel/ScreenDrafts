namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  public bool IsScheduled(DateTime utcNow) =>
    Status == DraftPartStatus.Created &&
     ScheduledForUtc.HasValue &&
      ScheduledForUtc.Value > utcNow;

  public DraftPartLifecycleView GetLifecycleView(DateTime utcNow) =>
    Status switch
    {
      _ when IsScheduled(utcNow) => DraftPartLifecycleView.Scheduled,
      _ when Status == DraftPartStatus.Created => DraftPartLifecycleView.Created,
      _ when Status == DraftPartStatus.InProgress => DraftPartLifecycleView.InProgress,
      _ when Status == DraftPartStatus.Completed => DraftPartLifecycleView.Completed,
      _ => DraftPartLifecycleView.Cancelled
    };

  internal Result Start()
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(DraftPartErrors.DraftCanOnlyBeStartedIfItIsCreated);
    }

    if (_draftPartParticipants.Count < 2)
    {
      return Result.Failure(DraftPartErrors.CannotStartADraftWithoutAtLeastTwoParticipants);
    }

    if (PrimaryHost is null || _draftHosts.Count == 0)
    {
      return Result.Failure(DraftPartErrors.CannotStartDraftWithoutAllHosts);
    }

    Status = DraftPartStatus.InProgress;

    Raise(new DraftPartStartedDomainEvent(
      Id.Value,
      DraftId.Value,
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
