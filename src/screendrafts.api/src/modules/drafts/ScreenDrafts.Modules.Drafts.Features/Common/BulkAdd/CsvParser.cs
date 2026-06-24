namespace ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd;

internal static class CsvParser
{
  /// <summary>
  /// Parses a headerless CSV where each row is: tmdbId,title
  /// Blank lines and lines where tmdbId cannot be parsed are recorded as failures.
  /// </summary>
  public static List<CsvRow> Parse(Stream csvStream)
  {
    using var reader = new StreamReader(csvStream);

    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
      HasHeaderRecord = false,
      MissingFieldFound = null,
      TrimOptions = TrimOptions.Trim,
    };

    using var csv = new CsvReader(reader, config);

    var rows = new List<CsvRow>();
    var rowNumber = 0;

    while (csv.Read())
    {
      rowNumber++;

      var rawTmdbId = csv.GetField<string?>(0);
      var title = csv.GetField<string?>(1);

      int? tmdbId = int.TryParse(rawTmdbId?.Trim(), out var parsed) && parsed > 0 ? parsed : null;

      rows.Add(new CsvRow(rowNumber, title?.Trim(), tmdbId));
    }

    return rows;
  }

  internal sealed record CsvRow(int RowNumber, string? Title, int? TmdbId);
}
