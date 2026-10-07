namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleHonorificSpec(
  TitleHonorificLevel Level,
  string? Search,
  string Sort,
  int Page,
  int PageSize);
