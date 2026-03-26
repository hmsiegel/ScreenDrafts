namespace ScreenDrafts.Modules.Movies.Features.Movies.SearchMedia;

internal sealed record SearchMediaRequest
{

  [FromRoute(Name = "draftPartId")]
  public required string DraftPartId { get; init; }

  [FromQuery(Name = "query")]
  public required string Query { get; init; }

  [FromQuery(Name = "year")]
  public int? Year { get; init; }

  [FromQuery(Name = "page")]
  public int Page { get; init; } = 1;

  [FromQuery(Name = "pageSize")]
  public int PageSize { get; init; } = 20;
}
