namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;
public sealed partial class Draft
{
  /// <summary>
  /// Mark a part as completed. If all parts are completed, the draft status is updated to Completed.
  /// </summary>
  /// <param name="partId">The part Id.</param>
  /// <returns>A result object.</returns>
  public Result CompletePart(DraftPartId partId)
  {
    ArgumentNullException.ThrowIfNull(partId);

    var part = FindPart(partId);

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }

    var result = part.Complete();

    if (result.IsFailure)
    {
      return result;
    }

    DeriveDraftStatus();

    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartCompletedDomainEvent(
      draftId: Id.Value,
      draftPartId: part.Id.Value,
      index: part.PartIndex));

    if (DraftStatus == DraftStatus.Completed)
    {
      Raise(new DraftCompletedDomainEvent(Id.Value));
    }
    return Result.Success();
  }

  public Result StartPart(DraftPartId partId)
  {
    ArgumentNullException.ThrowIfNull(partId);
    var part = FindPart(partId);
    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }
    var result = part.Start();
    if (result.IsFailure)
    {
      return result;
    }
    DeriveDraftStatus();
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartStartedDomainEvent(
      draftId: Id.Value,
      draftPartId: part.Id.Value,
      index: part.PartIndex));
    return Result.Success();
  }
}
