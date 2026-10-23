namespace ScreenDrafts.Modules.Integrations.Infrastructure.Services.ApplePodcasts;

internal sealed class ApplePodcastsSettings
{
  public const string SectionName = "Integrations:ApplePodcasts";

  public string CollectionId { get; set; } = default!;
  public string Country { get; set; } = "US";
  public string BaseAddress { get; set; } = "https://itunes.apple.com/";
}
