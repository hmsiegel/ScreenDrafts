namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;
public sealed partial class Draft
{
  public Result SetDraftersForPart(
    DraftPartId partId,
    IReadOnlyList<DrafterId> drafterIds)
  {
    ArgumentNullException.ThrowIfNull(drafterIds);
    ArgumentNullException.ThrowIfNull(partId);

    var part = FindPart(partId);
    List<ParticipantId> participantIds = new();

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }

    if (part.DraftId != Id)
    {
      return Result.Failure(DraftErrors.DraftPartDoesNotBelongToThisDraft);
    }

    if (drafterIds.Count > part.Draft.TotalDrafters)
    {
      return Result.Failure(DraftErrors.TooManyDrafters);
    }

    foreach (var drafter in drafterIds)
    {
      var participantId = ParticipantId.From(drafter);
      participantIds.Add(participantId);
    }

    return part.SetDrafters(participantIds);
  }

  public Result SetDrafterTeamsForPart(
    DraftPartId partId,
    IReadOnlyList<DrafterTeam> drafterTeamIds)
  {
    ArgumentNullException.ThrowIfNull(partId);
    ArgumentNullException.ThrowIfNull(drafterTeamIds);

    var part = FindPart(partId);

    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }

    if (part.DraftId != Id)
    {
      return Result.Failure(DraftErrors.DraftPartDoesNotBelongToThisDraft);
    }

    if (drafterTeamIds.Count > part.Draft.TotalDrafters)
    {
      return Result.Failure(DraftErrors.TooManyDrafterTeams);
    }

    return part.SetDrafterTeams(drafterTeamIds);
  }

  public Result SetHostsForPart(
    DraftPartId partId,
    IReadOnlyList<DraftHost> hosts)
  {
    ArgumentNullException.ThrowIfNull(hosts);
    ArgumentNullException.ThrowIfNull(partId);

    var part = FindPart(partId);
    if (part is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(partId.Value));
    }
    if (part.DraftId != Id)
    {
      return Result.Failure(DraftErrors.DraftPartDoesNotBelongToThisDraft);
    }
    if (hosts.Count > part.Draft.TotalHosts)
    {
      return Result.Failure(DraftErrors.TooManyHosts);
    }
    return part.SetHosts(hosts);
  }
}
