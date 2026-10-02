// Drafts module — Features/WikiExport/WikiExport.Common.cs
// Shared by ExportDrafts and ExportDrafters.

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.Common;

// ── Text helpers ──────────────────────────────────────────────────────────

internal static class WikiText
{
  internal const int MaxSelection = 50;

  // participant_kind_value: 0 = drafter, 1 = team, 2 = community
  internal const int CommunityKind = 2;
  internal const string PatreonMembers = "Patreon Members";

  private static readonly string[] Numerals =
  [
    "",
    "I",
    "II",
    "III",
    "IV",
    "V",
    "VI",
    "VII",
    "VIII",
    "IX",
    "X",
  ];

  internal static string Link(string name) => $"[[{name}]]";

  internal const string CommissionerOverrideNote = "removed via [[Commissioner Override]]";

  internal static string Roman(int n) =>
    n is > 0 and < 11 ? Numerals[n] : n.ToString(CultureInfo.InvariantCulture);

  internal static string Ordinal(int n)
  {
    var isTeen = (n % 100) is >= 11 and <= 13;

    var suffix = isTeen
      ? "th"
      : (n % 10) switch
      {
        1 => "st",
        2 => "nd",
        3 => "rd",
        _ => "th",
      };

    return $"{n.ToString(CultureInfo.InvariantCulture)}{suffix}";
  }

  // "A", "A and B", "A, B and C"
  internal static string JoinNames(IReadOnlyList<string> items)
  {
    ArgumentNullException.ThrowIfNull(items);

    return items.Count switch
    {
      0 => string.Empty,
      1 => items[0],
      2 => $"{items[0]} and {items[1]}",
      _ => $"{string.Join(", ", items.Take(items.Count - 1))} and {items[^1]}",
    };
  }

  // "" when the host covers every part, else " (Part III)" / " (Parts I and II)"
  internal static string PartSuffix(IReadOnlyCollection<int> partIndexes, int partCount)
  {
    var ordered = partIndexes.Distinct().OrderBy(i => i).ToList();

    if (ordered.Count == 0 || ordered.Count >= partCount)
    {
      return string.Empty;
    }

    var label = ordered.Count == 1 ? "Part" : "Parts";
    return $" ({label} {JoinNames([.. ordered.Select(Roman)])})";
  }

  internal static string FormatDate(DateOnly date) =>
    date.ToString("MMMM d, yyyy", CultureInfo.InvariantCulture);

  internal static string FirstToken(string displayName) =>
    displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? displayName;

  // ` | title             = value` — names padded to match the live wiki templates.
  internal static string Param(string name, string value) => $" | {name, -17} = {value}";

  internal static bool IsFinallyVetoed(IReadOnlyList<VetoRow> vetoes) =>
    vetoes.Count > 0 && !vetoes[^1].IsOverridden;

  // `<s>vetoed by [[A]]</s> veto overridden by [[B]]` for every overridden veto, in order.
  internal static string OverrideChain(IReadOnlyList<VetoRow> vetoes, Func<Guid?, string> nameOf)
  {
    ArgumentNullException.ThrowIfNull(vetoes);
    ArgumentNullException.ThrowIfNull(nameOf);

    return string.Join(
      " ",
      vetoes
        .Where(v => v.IsOverridden)
        .Select(v =>
          $"<s>vetoed by {Link(nameOf(v.IssuedByParticipantRowId))}</s> "
          + $"veto overridden by {Link(nameOf(v.OverriddenByParticipantRowId))}"
        )
    );
  }

  internal static ExportWikiResponse ToResponse(IReadOnlyList<WikiPage> pages, string kind)
  {
    ArgumentNullException.ThrowIfNull(pages);

    var sb = new StringBuilder();

    foreach (var page in pages)
    {
      sb.Append("=== Page: ").Append(page.Title).Append(" ===\n\n");
      sb.Append(page.Text.TrimEnd()).Append("\n\n\n");
    }

    return new ExportWikiResponse
    {
      FileName = $"screendrafts-wiki-{kind}-{DateTime.UtcNow:yyyyMMdd}.txt",
      Content = sb.ToString().TrimEnd() + "\n",
      PageCount = pages.Count,
    };
  }
}
