namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetRecordBook;

internal static class CopaceticDrafts
{
  /// <summary>
  /// A draft is copacetic when no drafter deploys a veto (overridden vetoes included) and no title
  /// is removed by commissioner override, across all of its parts.
  /// </summary>
  public static HashSet<Guid> Find(IEnumerable<DraftPartRow> parts)
  {
    ArgumentNullException.ThrowIfNull(parts);

    return
    [
      .. parts
        .GroupBy(p => p.DraftId)
        .Where(g => g.Sum(p => p.VetoesIssued) == 0 && g.Sum(p => p.CommissionerOverrides) == 0)
        .Select(g => g.Key),
    ];
  }
}
