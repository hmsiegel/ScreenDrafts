namespace ScreenDrafts.Modules.Drafts.Features.Series.List;

internal sealed class Summary : Summary<Endpoint>
{
  public Summary()
  {
    Summary = "Lists all series.";
    Description = "Returns a reference list of series, sorted by name. Administrators may pass an optional query parameter to include deleted series.";
    Response<SeriesCollectionResponse>(StatusCodes.Status200OK, "The list of series.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
