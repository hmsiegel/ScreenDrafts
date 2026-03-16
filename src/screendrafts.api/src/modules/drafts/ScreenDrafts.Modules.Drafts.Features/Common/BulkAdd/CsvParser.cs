namespace ScreenDrafts.Modules.Drafts.Features.Common.BulkAdd;

internal static class CsvParser
{
  public static List<CsvRow> Parse(Stream csvStream)
  {
    using var reader = new StreamReader(csvStream);
    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

    var rows = new List<CsvRow>();
    var rowNumber = 1;

    csv.Read();
    csv.ReadHeader();

    while (csv.Read())
    {
      rowNumber++;
      var title = csv.GetField<string?>("Title");
      var rawTmdbId = csv.GetField<string?>("TmdbId");

      int? tmdbId = int.TryParse(rawTmdbId, out var parsed) && parsed > 0
        ? parsed
        : null;

      rows.Add(new CsvRow(rowNumber, title, tmdbId));
    }

    return rows;
  }
  internal sealed record CsvRow(int RowNumber, string? Title, int? TmdbId);
}
