namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleHonorificResult(
  IReadOnlyList<TitleHonorificEntry> Titles,
  int TotalMatching,
  IReadOnlyList<TitleHonorificCount> Counts);
