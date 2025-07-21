namespace ScreenDrafts.Modules.Drafts.Application.People;
internal static class PeopleSqlQueryBuilder
{
  public static string BuildWhereClause(string? searchAlias = "p")
  {
    var prefix = string.IsNullOrWhiteSpace(searchAlias) ? "" : $"{searchAlias}.";
    return
      $"""
      WHERE
        (@Search IS NULL OR (
         {prefix}first_name ILIKE CONCAT('%', @Search, '%') OR
         {prefix}last_name ILIKE CONCAT('%', @Search, '%') OR
         {prefix}display_name ILIKE CONCAT('%', @Search, '%')))
      """;
  }

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "Reviewed")]
  public static string BuildOrderClause(string sortColumn, string sortOrder, string defaultSortColumn = "last_name")
  {
    var safeSort = sortColumn switch
    {
      "first_name" => "p.first_name",
      "last_name" => "p.last_name",
      "display_name" => "p.display_name",
      _ => $"{defaultSortColumn}"
    };

    var direction = sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase) ? "DESC" : "ASC";

    return $"ORDER BY {safeSort} {direction}";
  }

  public static string BuildPaginationClause => "LIMIT @PageSize OFFSET @Offset";
}
