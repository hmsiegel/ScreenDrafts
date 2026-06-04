namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts;

public sealed partial class DraftPart
{
  private readonly List<CommunityFilmRule> _communityFilmRules = [];

  public IReadOnlyList<CommunityFilmRule> CommunityFilmRules => _communityFilmRules.AsReadOnly();

  public Result AddCommunityFilmRule(
    string publicId,
    CommunityFilmRuleKind ruleKind,
    int? targetSlot
  )
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (ruleKind == CommunityFilmRuleKind.BoostersPick && (targetSlot is null || targetSlot <= 0))
    {
      return Result.Failure(CommunityFilmRuleErrors.TargetSlotRequired);
    }

    var result = CommunityFilmRule.Create(
      ruleKind: ruleKind,
      targetSlot: ruleKind == CommunityFilmRuleKind.BoostersPick ? targetSlot : null,
      publicId: publicId
    );

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    var rule = result.Value;
    _communityFilmRules.Add(rule);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result UpdateCommunityFilmRule(
    string publicId,
    CommunityFilmRuleKind ruleKind,
    int? targetSlot
  )
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (ruleKind == CommunityFilmRuleKind.BoostersPick && (targetSlot is null || targetSlot <= 0))
    {
      return Result.Failure(CommunityFilmRuleErrors.TargetSlotRequired);
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }

    rule.Update(
      ruleKind: ruleKind,
      targetSlot: ruleKind == CommunityFilmRuleKind.BoostersPick ? targetSlot : null
    );

    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result RemoveCommunityFilmRule(string publicId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }

    _communityFilmRules.Remove(rule);
    UpdatedAtUtc = DateTime.UtcNow;

    return Result.Success();
  }

  public Result AssignFilmToCommunityFilmRule(string publicId, int tmdbId)
  {
    if (Status != DraftPartStatus.Created)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotModifiable);
    }

    if (_communityFilmRules.Any(r => r.TmdbId == tmdbId && r.PublicId != publicId))
    {
      return Result.Failure(CommunityFilmRuleErrors.FilmAlreadyAssigned(tmdbId));
    }

    var rule = _communityFilmRules.FirstOrDefault(r => r.PublicId == publicId);

    if (rule is null)
    {
      return Result.Failure(CommunityFilmRuleErrors.NotFound(publicId));
    }
    rule.AssignFilm(tmdbId);
    UpdatedAtUtc = DateTime.UtcNow;
    return Result.Success();
  }
}
