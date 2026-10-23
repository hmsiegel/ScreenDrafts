using ScreenDrafts.Common.Abstractions.Errors;

namespace ScreenDrafts.Modules.Integrations.Domain.Services.Podcasts;

public static class PodcastErrors
{
  public static SDError RateLimited(string platform) =>
    SDError.Problem(
      "Podcasts.RateLimited",
      $"{platform} rejected the request because a rate limit or quota was exceeded."
    );

  public static SDError RequestFailed(string platform, string message) =>
    SDError.Problem("Podcasts.RequestFailed", $"The {platform} request failed: {message}");
}
