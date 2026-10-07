namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record GetTitleHonorificsQuery : IQuery<GetTitleHonorificsResponse>
{
  public required string Level { get; init; }
  public string? Search { get; init; }
  public string? Sort { get; init; }
  public int? Page { get; init; }
  public int? PageSize { get; init; }
  public required bool IncludeAll { get; init; }
}
