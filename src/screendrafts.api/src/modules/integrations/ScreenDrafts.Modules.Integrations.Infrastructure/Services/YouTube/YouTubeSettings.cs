namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.YouTube;

internal sealed class YouTubeSettings
{
  public const string SectionName = "Integrations:YouTube";

  public string Key { get; set; } = default!;
  public string BaseAddress { get; set; } = default!;
}
