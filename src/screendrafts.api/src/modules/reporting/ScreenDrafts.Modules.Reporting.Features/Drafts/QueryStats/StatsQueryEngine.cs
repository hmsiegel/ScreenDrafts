namespace ScreenDrafts.Modules.Reporting.Features.Drafts.QueryStats;

/// <summary>
/// Runs a validated query over the in-memory dataset. Filters pick out whole drafts (series, draft type,
/// episode range), so every metric is computed over complete drafts. Descending sorts drop zero-valued
/// groups; ascending sorts keep them, so "fewest" queries show who has none.
/// </summary>
internal static class StatsQueryEngine
{
  private const int DrafterKind = 0;

  public static StatsQueryResult Run(QueryDataset data, StatsQuerySpec spec)
  {
    ArgumentNullException.ThrowIfNull(data);
    ArgumentNullException.ThrowIfNull(spec);

    var picks = data.Picks.Where(p => Matches(p, spec)).ToList();
    var pickById = picks.ToDictionary(p => p.PickId);
    var credits = data.Credits.Where(c => pickById.ContainsKey(c.PickId)).ToList();
    var vetoes = data.Vetoes.Where(v => pickById.ContainsKey(v.PickId)).ToList();

    var scope = new Scope(picks, credits, vetoes, pickById, FindCopacetic(picks, vetoes, pickById));

    var universe =
      spec.GroupBy == StatsGroupBys.Drafter
        ? DrafterGroups(scope)
        : PickGroups(scope, spec.GroupBy);
    var values = ComputeValues(scope, spec.Metric, spec.GroupBy);

    var scored = universe
      .Where(g => spec.MinAppearances is null || (g.Value.DraftCount ?? 0) >= spec.MinAppearances)
      .Select(g => (g.Key, g.Value, Amount: Math.Round(values.GetValueOrDefault(g.Key), 4)));

    if (!spec.Ascending)
    {
      scored = scored.Where(g => g.Amount > 0);
    }

    var ordered = (
      spec.Ascending ? scored.OrderBy(g => g.Amount) : scored.OrderByDescending(g => g.Amount)
    )
      .ThenBy(g => g.Value.Name, StringComparer.OrdinalIgnoreCase)
      .ToList();

    var rows = ordered
      .Take(spec.Limit)
      .Select(g => new QueryStatsRow
      {
        Rank = 1 + ordered.Count(o => spec.Ascending ? o.Amount < g.Amount : o.Amount > g.Amount),
        Name = g.Value.Name,
        PublicId = g.Value.PublicId,
        Value = g.Amount,
        Context = DraftCountLabel(g.Value.DraftCount),
      })
      .ToList();

    return new StatsQueryResult(rows, ordered.Count);
  }

  private static string? DraftCountLabel(int? draftCount)
  {
    if (draftCount is null)
    {
      return null;
    }

    return draftCount == 1 ? "1 draft" : $"{draftCount} drafts";
  }

  private static bool Matches(QueryPickRow p, StatsQuerySpec spec) =>
    (spec.Series.Count == 0 || spec.Series.Contains(p.SeriesName))
    && (spec.DraftTypes.Count == 0 || spec.DraftTypes.Contains(p.DraftType))
    && (spec.EpisodeFrom is null || p.EpisodeNumber >= spec.EpisodeFrom)
    && (spec.EpisodeTo is null || p.EpisodeNumber <= spec.EpisodeTo);

  /// <summary>Copacetic: no veto of any kind and no commissioner override anywhere in the draft.</summary>
  private static HashSet<Guid> FindCopacetic(
    List<QueryPickRow> picks,
    List<QueryVetoRow> vetoes,
    Dictionary<Guid, QueryPickRow> pickById
  )
  {
    var dirty = picks
      .Where(p => p.CommissionerOverridden)
      .Select(p => p.DraftId)
      .Concat(vetoes.Select(v => pickById[v.PickId].DraftId))
      .ToHashSet();

    return [.. picks.Select(p => p.DraftId).Where(id => !dirty.Contains(id))];
  }

  private static Dictionary<string, GroupInfo> DrafterGroups(Scope s) =>
    s
      .Credits.GroupBy(c => c.DrafterId)
      .ToDictionary(
        g => g.Key.ToString(),
        g => new GroupInfo(
          g.First().DrafterName,
          g.First().DrafterPublicId,
          g.Select(c => s.PickById[c.PickId].DraftId).Distinct().Count()
        )
      );

  private static Dictionary<string, GroupInfo> PickGroups(Scope s, string groupBy) =>
    s
      .Picks.GroupBy(p => PickKey(groupBy, p))
      .ToDictionary(g => g.Key, g => PickInfo(groupBy, g.First()));

  private static string PickKey(string groupBy, QueryPickRow p) =>
    groupBy switch
    {
      StatsGroupBys.Draft => p.DraftId.ToString(),
      StatsGroupBys.Series => p.SeriesName,
      StatsGroupBys.DraftType => p.DraftType,
      StatsGroupBys.Title => p.MediaPublicId,
      _ => throw new ArgumentOutOfRangeException(
        nameof(groupBy),
        groupBy,
        "Not a pick-level group-by."
      ),
    };

  private static GroupInfo PickInfo(string groupBy, QueryPickRow p) =>
    groupBy switch
    {
      StatsGroupBys.Draft => new GroupInfo(p.DraftTitle, p.DraftPublicId, null),
      StatsGroupBys.Series => new GroupInfo(p.SeriesName, null, null),
      StatsGroupBys.DraftType => new GroupInfo(p.DraftType, null, null),
      StatsGroupBys.Title => new GroupInfo(p.MediaTitle, p.MediaPublicId, null),
      _ => throw new ArgumentOutOfRangeException(
        nameof(groupBy),
        groupBy,
        "Not a pick-level group-by."
      ),
    };

  private static Dictionary<string, decimal> ComputeValues(
    Scope s,
    string metric,
    string groupBy
  ) =>
    metric switch
    {
      StatsMetrics.TitlesDrafted => CountPicks(s, groupBy, p => p.Landed),
      StatsMetrics.PicksVetoed => CountPicks(s, groupBy, p => p.VetoStanding),
      StatsMetrics.CommissionerOverrides => CountPicks(s, groupBy, p => p.CommissionerOverridden),
      StatsMetrics.VetoesUsed => CountVetoes(s, groupBy, _ => true),
      StatsMetrics.VetoesOverridden => CountVetoes(s, groupBy, v => v.IsOverridden),
      StatsMetrics.SelfVetoes => CountVetoes(s, groupBy, v => v.IsSelfVeto && !v.IsOverridden),
      StatsMetrics.Appearances => Appearances(s, groupBy),
      StatsMetrics.CopaceticDrafts => CopaceticDrafts(s, groupBy),
      StatsMetrics.AvgVetoesPerDraft => AvgVetoesPerDraft(s, groupBy),
      _ => new Dictionary<string, decimal>(),
    };

  private static Dictionary<string, decimal> CountPicks(
    Scope s,
    string groupBy,
    Func<QueryPickRow, bool> predicate
  ) =>
    groupBy == StatsGroupBys.Drafter
      ? s
        .Credits.Where(c => predicate(s.PickById[c.PickId]))
        .GroupBy(c => c.DrafterId.ToString())
        .ToDictionary(g => g.Key, g => (decimal)g.Count())
      : s
        .Picks.Where(predicate)
        .GroupBy(p => PickKey(groupBy, p))
        .ToDictionary(g => g.Key, g => (decimal)g.Count());

  private static Dictionary<string, decimal> CountVetoes(
    Scope s,
    string groupBy,
    Func<QueryVetoRow, bool> predicate
  ) =>
    groupBy == StatsGroupBys.Drafter
      ? s
        .Vetoes.Where(v => v.IssuedByKind == DrafterKind && predicate(v))
        .GroupBy(v => v.IssuedByIdValue.ToString())
        .ToDictionary(g => g.Key, g => (decimal)g.Count())
      : s
        .Vetoes.Where(predicate)
        .GroupBy(v => PickKey(groupBy, s.PickById[v.PickId]))
        .ToDictionary(g => g.Key, g => (decimal)g.Count());

  /// <summary>Drafters: distinct drafts. Series and draft types: distinct (drafter, draft) pairs.</summary>
  private static Dictionary<string, decimal> Appearances(Scope s, string groupBy) =>
    groupBy == StatsGroupBys.Drafter
      ? s
        .Credits.GroupBy(c => c.DrafterId.ToString())
        .ToDictionary(
          g => g.Key,
          g => (decimal)g.Select(c => s.PickById[c.PickId].DraftId).Distinct().Count()
        )
      : s
        .Credits.GroupBy(c => PickKey(groupBy, s.PickById[c.PickId]))
        .ToDictionary(
          g => g.Key,
          g =>
            (decimal)g.Select(c => (s.PickById[c.PickId].DraftId, c.DrafterId)).Distinct().Count()
        );

  private static Dictionary<string, decimal> CopaceticDrafts(Scope s, string groupBy) =>
    groupBy == StatsGroupBys.Drafter
      ? s
        .Credits.Where(c => s.Copacetic.Contains(s.PickById[c.PickId].DraftId))
        .GroupBy(c => c.DrafterId.ToString())
        .ToDictionary(
          g => g.Key,
          g => (decimal)g.Select(c => s.PickById[c.PickId].DraftId).Distinct().Count()
        )
      : s
        .Picks.Where(p => s.Copacetic.Contains(p.DraftId))
        .GroupBy(p => PickKey(groupBy, p))
        .ToDictionary(g => g.Key, g => (decimal)g.Select(p => p.DraftId).Distinct().Count());

  /// <summary>Drafters: vetoes they issued over their appearances. Series and draft types: all vetoes over drafts.</summary>
  private static Dictionary<string, decimal> AvgVetoesPerDraft(Scope s, string groupBy)
  {
    var vetoes = CountVetoes(s, groupBy, _ => true);

    var denominators =
      groupBy == StatsGroupBys.Drafter
        ? Appearances(s, groupBy)
        : s
          .Picks.GroupBy(p => PickKey(groupBy, p))
          .ToDictionary(g => g.Key, g => (decimal)g.Select(p => p.DraftId).Distinct().Count());

    return denominators
      .Where(d => d.Value > 0)
      .ToDictionary(d => d.Key, d => vetoes.GetValueOrDefault(d.Key) / d.Value);
  }

  private sealed record Scope(
    List<QueryPickRow> Picks,
    List<QueryCreditRow> Credits,
    List<QueryVetoRow> Vetoes,
    Dictionary<Guid, QueryPickRow> PickById,
    HashSet<Guid> Copacetic
  );

  private sealed record GroupInfo(string Name, string? PublicId, int? DraftCount);
}
