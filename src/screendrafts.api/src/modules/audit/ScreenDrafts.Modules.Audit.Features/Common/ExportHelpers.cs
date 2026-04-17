namespace ScreenDrafts.Modules.Audit.Features.Common;

internal sealed class ExportHelpers<T>(IHttpContextAccessor httpContextAccessor)
  where T : class
{
  private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

  public async Task WriteCsvAsync(
    IEnumerable<T> rows,
    string fileName,
    CancellationToken ct)
  {
    var httpContext = _httpContextAccessor.HttpContext;
    if (httpContext != null)
    {
      httpContext.Response.ContentType = "text/csv";
      httpContext.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileName}\"");

      await using var writer = new StreamWriter(httpContext.Response.Body);
      await using var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture));

      await csv.WriteRecordsAsync(rows, ct);
    }
  }
}
