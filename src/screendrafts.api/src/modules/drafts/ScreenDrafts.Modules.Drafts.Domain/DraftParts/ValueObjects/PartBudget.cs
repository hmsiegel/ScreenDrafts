namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

/// <summary>
/// Per-part budget for vetoes and veto overrides computed from series rules.
/// </summary>
/// <param name="MaxVetoes">The maximum number of vetoes for this draft part.</param>
/// <param name="MaxVetoOverrides">The maximum number of veto overrides for this draft part.</param>
public sealed record PartBudget(int MaxVetoes, int MaxVetoOverrides, int MaxCommunityPicks = 0, int MaxCommunityVetoes = 0);
