using System.Text.RegularExpressions;

namespace ScreenDrafts.Modules.Drafts.Features.WikiExport.Common;

internal sealed record ArrangeEntry(string Title, int? TmdbId, int? OrderIndex, bool? IsCorrect);

internal sealed record ArrangeSet(string Name, IReadOnlyList<ArrangeEntry> Entries);

/// <param name="Rank">List slot for ranked rows; null for unranked rows.</param>
/// <param name="Cells">One cell per input set, in input order. Null when that contestant has no entry on the row.</param>
internal sealed record ArrangedRow(int? Rank, IReadOnlyList<ArrangeEntry?> Cells);

internal sealed record ArrangedPredictions(bool Ranked, IReadOnlyList<ArrangedRow> Rows);

/// <summary>
/// Row layout for prediction results. Mirrors the UI's prediction-rows.ts — keep the two in step.
///
/// Ranked predictions (the caller says so, from the part's prediction mode): one row per list slot,
/// highest slot number first (7 to 1). Unranked entries can still carry an OrderIndex.
///
/// Unranked predictions, top to bottom:
///   1. Films several contestants predicted and got right (same row).
///   2. Films only the top scorer got right, each paired with a miss no one else predicted.
///   3. Same for the next scorer down.
///   4. Films several contestants predicted and missed (same row).
///   5. Remaining solo misses, paired row by row.
///
/// Top scorer = most correct entries; ties go to Ryan, then input order.
/// </summary>
internal static partial class PredictionRowArranger
{
  public static ArrangedPredictions Arrange(IReadOnlyList<ArrangeSet> sets, bool rankedMode)
  {
    ArgumentNullException.ThrowIfNull(sets);

    var all = sets.SelectMany(s => s.Entries).ToList();

    if (all.Count == 0)
    {
      return new ArrangedPredictions(false, []);
    }

    return rankedMode && all.All(e => e.OrderIndex.HasValue)
      ? ArrangeRanked(sets)
      : new ArrangedPredictions(false, ArrangeUnranked(sets));
  }

  private static ArrangedPredictions ArrangeRanked(IReadOnlyList<ArrangeSet> sets)
  {
    var slots = sets.SelectMany(s => s.Entries).Select(e => e.OrderIndex!.Value).Distinct();
    var ordered = slots.OrderByDescending(x => x);

    var rows = ordered
      .Select(slot => new ArrangedRow(
        slot,
        [.. sets.Select(s => s.Entries.FirstOrDefault(e => e.OrderIndex == slot))]
      ))
      .ToList();

    return new ArrangedPredictions(true, rows);
  }

  private static List<ArrangedRow> ArrangeUnranked(IReadOnlyList<ArrangeSet> sets)
  {
    // Merge entries into films so one title shares one row across contestants.
    var films = new Dictionary<string, Film>();

    for (var c = 0; c < sets.Count; c++)
    {
      foreach (var entry in sets[c].Entries)
      {
        var key = FilmKey(entry);

        if (!films.TryGetValue(key, out var film))
        {
          film = new Film(films.Count);
          films[key] = film;
        }

        film.ByContestant.TryAdd(c, entry);

        if (entry.IsCorrect == true)
        {
          film.Correct = true;
        }
      }
    }

    // Top scorer first; ties go to Ryan, then input order.
    var correctCount = sets.Select(s => s.Entries.Count(e => e.IsCorrect == true)).ToArray();
    var rankOrder = Enumerable
      .Range(0, sets.Count)
      .OrderByDescending(i => correctCount[i])
      .ThenByDescending(i => IsRyan(sets[i].Name))
      .ThenBy(i => i)
      .ToList();

    var shared = films
      .Values.Where(f => f.ByContestant.Count >= 2)
      .OrderByDescending(f => f.ByContestant.Count)
      .ThenBy(f => f.ByContestant.Keys.Min(c => rankOrder.IndexOf(c)))
      .ThenBy(f => f.Order)
      .ToList();

    var solos = films.Values.Where(f => f.ByContestant.Count == 1).OrderBy(f => f.Order).ToList();

    var soloCorrect = sets
      .Select(
        (_, c) =>
          new Queue<ArrangeEntry>(
            solos.Where(f => f.Correct && f.ByContestant.ContainsKey(c)).Select(f => f.ByContestant[c])
          )
      )
      .ToArray();

    var soloWrong = sets
      .Select(
        (_, c) =>
          new Queue<ArrangeEntry>(
            solos.Where(f => !f.Correct && f.ByContestant.ContainsKey(c)).Select(f => f.ByContestant[c])
          )
      )
      .ToArray();

    var rows = new List<ArrangedRow>();

    void AddShared(Film film)
    {
      var cells = new ArrangeEntry?[sets.Count];

      foreach (var (c, entry) in film.ByContestant)
      {
        cells[c] = entry;
      }

      rows.Add(new ArrangedRow(null, cells));
    }

    // 1. Shared hits.
    foreach (var film in shared.Where(f => f.Correct))
    {
      AddShared(film);
    }

    // 2-3. Solo hits, top scorer first, each paired with other contestants' solo misses.
    foreach (var c in rankOrder)
    {
      while (soloCorrect[c].Count > 0)
      {
        var cells = new ArrangeEntry?[sets.Count];
        cells[c] = soloCorrect[c].Dequeue();

        foreach (var other in rankOrder)
        {
          if (other != c && soloWrong[other].Count > 0)
          {
            cells[other] = soloWrong[other].Dequeue();
          }
        }

        rows.Add(new ArrangedRow(null, cells));
      }
    }

    // 4. Shared misses.
    foreach (var film in shared.Where(f => !f.Correct))
    {
      AddShared(film);
    }

    // 5. Leftover solo misses, paired.
    while (soloWrong.Any(q => q.Count > 0))
    {
      var cells = new ArrangeEntry?[sets.Count];

      for (var c = 0; c < soloWrong.Length; c++)
      {
        if (soloWrong[c].Count > 0)
        {
          cells[c] = soloWrong[c].Dequeue();
        }
      }

      rows.Add(new ArrangedRow(null, cells));
    }

    return rows;
  }

  private static string FilmKey(ArrangeEntry e) =>
    e.TmdbId is { } id ? $"id:{id}" : $"t:{e.Title.Trim().ToUpperInvariant()}";

  private static bool IsRyan(string name) => RyanRegex().IsMatch(name.Trim());

  [GeneratedRegex(@"^ryan\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
  private static partial Regex RyanRegex();

  private sealed class Film(int order)
  {
    public int Order { get; } = order;
    public Dictionary<int, ArrangeEntry> ByContestant { get; } = [];
    public bool Correct { get; set; }
  }
}
