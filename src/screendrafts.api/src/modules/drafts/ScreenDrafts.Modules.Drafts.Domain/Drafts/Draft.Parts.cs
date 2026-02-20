namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;
public sealed partial class Draft
{
  /// <summary>
  /// Adds a draft part with the specified parameters.
  /// </summary>
  /// <param name="partIndex">The draft part index.</param>
  /// <returns>The draft part.</returns>
  public Result<DraftPartId> AddPart(int partIndex, int min, int max)
  {
    if (partIndex <= 0)
    {
      return Result.Failure<DraftPartId>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    if (_parts.Any(p => p.PartIndex == partIndex))
    {
      return Result.Failure<DraftPartId>(DraftErrors.DraftPartWithIndexAlreadyExists(partIndex));
    }

    var gameplayResult = DraftPartGamePlaySnapshot.Create(
      draftType: DraftType,
      seriesId: SeriesId,
      minPosition: min,
      maxPosition: max);

    if (gameplayResult.IsFailure)
    {
      return Result.Failure<DraftPartId>(gameplayResult.Error!);
    }

    var partResult = DraftPart.Create(
      draftId: Id,
      partIndex: partIndex,
      gameplay: gameplayResult.Value);

    if (partResult.IsFailure)
    {
      return Result.Failure<DraftPartId>(partResult.Error!);
    }

    _parts.Add(partResult.Value);

    RenumberDraftParts();

    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartAddedDomainEvent(
      draftId: Id.Value,
      partId: partResult.Value.Id.Value,
      partIndex: partResult.Value.PartIndex));
    return Result.Success(partResult.Value.Id);
  }


  public Result RemovePart(DraftPartId partId)
  {
    ArgumentNullException.ThrowIfNull(partId);

    var foundPart = FindPart(partId);

    if (foundPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }

    if (foundPart.Picks.Count > 0)
    {
      return Result.Failure(DraftErrors.CannotRemoveDraftPartWithPicks);
    }

    _parts.Remove(foundPart);
    RenumberContiguousParts();
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartRemovedDomainEvent(
      Id.Value,
      foundPart.Id.Value));
    return Result.Success();
  }

  public void RenumberDraftParts()
  {
    if (_parts.Count == 0)
    {
      return;
    }

    var ordered = _parts
      .OrderBy(p => p.PartIndex)
      .ToList();

    for (int i = 0; i < ordered.Count; i++)
    {
      var expected = i + 1;
      if (ordered[i].PartIndex != expected)
      {
        ordered[i].SetPartIndex(expected);
      }
    }

    SortPartsByNumber();
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartsRenumberedDomainEvent(Id.Value));
  }

  private void SortPartsByNumber()
  {
    _parts.Sort((x, y) => x.PartIndex.CompareTo(y.PartIndex));
  }

  private DraftPart? FindPart(DraftPartId partId) =>
    _parts.FirstOrDefault(p => p.Id == partId);

  private void RenumberContiguousParts()
  {
    for (int i = 0; i < _parts.Count; i++)
    {
      _parts[i].SetPartIndex(i + 1);
    }
  }
}
