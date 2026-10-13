namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

/// <param name="MinAppearances">The fewest appearances a title needs to hold the level.</param>
internal sealed record TitleHonorificLevel(
  string Code,
  string Label,
  int MinAppearances,
  TitleHonorificKind Kind = TitleHonorificKind.Appearances);
