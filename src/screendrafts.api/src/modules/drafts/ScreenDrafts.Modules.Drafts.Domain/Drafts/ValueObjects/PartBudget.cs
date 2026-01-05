namespace ScreenDrafts.Modules.Drafts.Domain.Drafts.ValueObjects;

/// <summary>
/// Per-part budget for vetoes and veto overrides computed from series rules.
/// </summary>
/// <param name="MaxVetoes">The maximum number of vetoes for this draft part.</param>
/// <param name="MaxVetoOverriedes">The maximum number of veto overrides for this draft part.</param>
public sealed record PartBudget(int MaxVetoes, int MaxVetoOverriedes, int MaxCommunityVetoes = 0, int MaxCommunityPicks = 0);
