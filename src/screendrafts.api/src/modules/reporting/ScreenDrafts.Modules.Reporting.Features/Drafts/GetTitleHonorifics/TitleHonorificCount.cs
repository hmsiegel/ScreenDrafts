namespace ScreenDrafts.Modules.Reporting.Features.Drafts.GetTitleHonorifics;

internal sealed record TitleHonorificCount
{
  public required string Code { get; init; }
  public required string Label { get; init; }
  public required int Count { get; init; }
}
