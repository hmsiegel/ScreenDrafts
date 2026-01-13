namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;
public sealed partial class Draft
{
  /// <summary>
  /// Adds a draft part with the specified parameters.
  /// </summary>
  /// <param name="partIndex">The draft part index.</param>
  /// <param name="totalPicks">The total number of picks for the part.</param>
  /// <param name="totalDrafters">The total number of drafters for the part.</param>
  /// <param name="totalDrafterTeams">The total number of drafter teams for the part.</param>
  /// <param name="totalHosts">The total number of hosts for the part.</param>
  /// <returns>The draft part.</returns>
  public Result<DraftPart> AddPart(
    int partIndex,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts)
  {
    if (partIndex <= 0)
    {
      return Result.Failure<DraftPart>(DraftErrors.PartIndexMustBeGreaterThanZero);
    }

    if (_parts.Any(p => p.PartIndex == partIndex))
    {
      return Result.Failure<DraftPart>(DraftErrors.DraftPartWithIndexAlreadyExists(partIndex));
    }

    var part = DraftPart.Create(
      draft: this,
      partIndex: partIndex,
      totalPicks: totalPicks,
      totalDrafters: totalDrafters,
      totalDrafterTeams: totalDrafterTeams,
      totalHosts: totalHosts).Value;
    _parts.Add(part);

    if (_parts.Count > 1)
    {
      SortPartsByNumber();
    }

    Raise(new DraftPartAddedDomainEvent(
      draftId: Id.Value,
      partId: part.Id.Value,
      partIndex: partIndex));
    return part;
  }


  public Result RemovePart(DraftPartId partId)
  {
    ArgumentNullException.ThrowIfNull(partId);

    var foundPart = FindPart(partId);

    if (foundPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }

    if (foundPart.DraftId != Id)
    {
      return Result.Failure(DraftErrors.DraftPartDoesNotBelongToThisDraft);
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

  public Result RenumberDraftParts(IReadOnlyList<DraftPartId> orderedPartList)
  {
    ArgumentNullException.ThrowIfNull(orderedPartList);

    if (orderedPartList.Count != _parts.Count)
    {
      return Result.Failure(DraftErrors.DraftPartsCountMismatch);
    }

    var partIdsSet = _parts.Select(p => p.Id).ToHashSet();

    var orderedPartIdsSet = orderedPartList.ToHashSet();

    if (!partIdsSet.SetEquals(orderedPartIdsSet))
    {
      return Result.Failure(DraftErrors.DraftPartsMismatch);
    }

    for (int i = 0; i < orderedPartList.Count; i++)
    {
      var partId = orderedPartList[i];
      var part = FindPart(partId);
      part?.SetPartIndex(i + 1);
    }

    SortPartsByNumber();
    UpdatedAtUtc = DateTime.UtcNow;
    Raise(new DraftPartsRenumberedDomainEvent(Id.Value));
    return Result.Success();
  }

  private void SortPartsByNumber()
  {
    _parts.Sort((x, y) => x.PartIndex.CompareTo(y.PartIndex));
  }

  private DraftPart? FindPart(DraftPartId partId) =>
    _parts.FirstOrDefault(p => p.Id == partId);

  private Host? FindHost(HostId hostId, DraftPartId draftPartId)
  {
    var part = FindPart(draftPartId);
    var host = part?.DraftHosts.FirstOrDefault(h => h.HostId == hostId);

    return host?.Host;
  }

  private void RenumberContiguousParts()
  {
    for (int i = 0; i < _parts.Count; i++)
    {
      _parts[i].SetPartIndex(i + 1);
    }
  }
}
