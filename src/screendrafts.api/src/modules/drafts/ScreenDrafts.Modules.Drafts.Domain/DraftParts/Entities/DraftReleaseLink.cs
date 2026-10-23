namespace ScreenDrafts.Modules.Drafts.Domain.DraftParts.Entities;

/// <summary>
/// The episode link for one release on one podcast platform. Lives under its
/// <see cref="DraftRelease"/>; created for MainFeed releases only.
/// </summary>
public sealed class DraftReleaseLink
{
  private DraftReleaseLink(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    ReleaseLinkPlatform platform,
    DateTime nowUtc
  )
  {
    PartId = partId;
    ReleaseChannel = releaseChannel;
    Platform = platform;
    Status = ReleaseLinkStatus.Pending;
    NextCheckAtUtc = nowUtc;
    UpdatedOnUtc = nowUtc;
  }

  private DraftReleaseLink()
  {
    // For EF
  }

  public DraftPartId PartId { get; private set; } = default!;
  public ReleaseChannel ReleaseChannel { get; private set; } = default!;
  public ReleaseLinkPlatform Platform { get; private set; } = default!;
  public DraftRelease Release { get; private set; } = default!;
  public Uri? Url { get; private set; }
  public ReleaseLinkStatus Status { get; private set; } = default!;
  public DateTime? NextCheckAtUtc { get; private set; }
  public int Attempts { get; private set; }
  public DateTime UpdatedOnUtc { get; private set; }

  internal static DraftReleaseLink CreatePending(
    DraftPartId partId,
    ReleaseChannel releaseChannel,
    ReleaseLinkPlatform platform,
    DateTime nowUtc
  ) => new(partId, releaseChannel, platform, nowUtc);

  public Result MarkLinked(Uri url, DateTime nowUtc)
  {
    if (!IsValidUrl(Platform, url))
    {
      return Result.Failure(ReleaseLinkErrors.InvalidUrl(Platform.Name));
    }

    Url = url;
    Status = ReleaseLinkStatus.Linked;
    NextCheckAtUtc = null;
    UpdatedOnUtc = nowUtc;

    return Result.Success();
  }

  public void MarkNeedsReview(DateTime nowUtc)
  {
    Status = ReleaseLinkStatus.NeedsReview;
    NextCheckAtUtc = null;
    UpdatedOnUtc = nowUtc;
  }

  public void MarkNotFound(DateTime nowUtc)
  {
    Status = ReleaseLinkStatus.NotFound;
    NextCheckAtUtc = null;
    UpdatedOnUtc = nowUtc;
  }

  public void ScheduleRetry(DateTime nextCheckAtUtc, bool countsAsAttempt, DateTime nowUtc)
  {
    Status = ReleaseLinkStatus.Pending;
    NextCheckAtUtc = nextCheckAtUtc;
    UpdatedOnUtc = nowUtc;

    if (countsAsAttempt)
    {
      Attempts++;
    }
  }

  /// <summary>
  /// A changed release date invalidates earlier lookups, so anything not yet
  /// linked is searched again. A <see cref="ReleaseLinkStatus.Linked"/> link stays.
  /// </summary>
  internal void ResetToPending(DateTime nowUtc)
  {
    if (Status == ReleaseLinkStatus.Linked)
    {
      return;
    }

    Status = ReleaseLinkStatus.Pending;
    Url = null;
    Attempts = 0;
    NextCheckAtUtc = nowUtc;
    UpdatedOnUtc = nowUtc;
  }

  private static bool IsValidUrl(ReleaseLinkPlatform platform, Uri? url)
  {
    if (url is null || !url.IsAbsoluteUri || url.Scheme != Uri.UriSchemeHttps)
    {
      return false;
    }

    return platform == ReleaseLinkPlatform.Spotify
      ? url.Host == "open.spotify.com"
        && url.AbsolutePath.StartsWith("/episode/", StringComparison.Ordinal)
      : url.Host == "podcasts.apple.com";
  }
}
