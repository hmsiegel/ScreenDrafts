namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

public sealed class CommunityFilmRule : Entity
{
  private CommunityFilmRule(
    CommunityFilmRuleKind ruleKind,
    int? targetSlot,
    string publicId,
    Guid? id = null
  )
    : base(id ?? Guid.NewGuid())
  {
    RuleKind = ruleKind;
    TargetSlot = targetSlot;
    PublicId = publicId;
  }

  private CommunityFilmRule() { }

  public string PublicId { get; private set; } = default!;
  public int? TmdbId { get; private set; }
  public CommunityFilmRuleKind RuleKind { get; private set; } = default!;
  public int? TargetSlot { get; private set; }

  /// <summary>
  /// True once the system auto-veto has fired for a BoostersVeto rule.
  /// BoostersVeto gets exactly one auto-veto; subsequent out-of-slot plays
  /// are not vetoed again. Always false for BoostersPick rules (which enforce
  /// every play until the film lands at TargetSlot).
  /// </summary>
  public bool WasAutoVetoFired { get; private set; }

  public static Result<CommunityFilmRule> Create(
    CommunityFilmRuleKind ruleKind,
    int? targetSlot,
    string publicId,
    Guid? id = null
  )
  {
    var communityFilmRule = new CommunityFilmRule(
      ruleKind: ruleKind,
      targetSlot: targetSlot,
      publicId: publicId,
      id: id
    );

    return Result.Success(communityFilmRule);
  }

  internal void Update(CommunityFilmRuleKind ruleKind, int? targetSlot)
  {
    RuleKind = ruleKind;
    TargetSlot = targetSlot;
  }

  internal void AssignFilm(int tmdbId)
  {
    TmdbId = tmdbId;
  }

  internal void ClearFilm()
  {
    TmdbId = null;
  }

  /// <summary>
  /// Marks this BoostersVeto rule's single auto-veto as spent.
  /// Has no effect on BoostersPick rules.
  /// </summary>
  internal void MarkAutoVetoFired()
  {
    WasAutoVetoFired = true;
  }
}
