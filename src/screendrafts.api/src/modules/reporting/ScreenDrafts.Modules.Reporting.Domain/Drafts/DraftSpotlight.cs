namespace ScreenDrafts.Modules.Reporting.Domain.Drafts;

public sealed class DraftSpotlight : Entity<DraftSpotlightId>
{
  private DraftSpotlight(
    string publicId,
    string draftPublicId,
    string spotlightDescription,
    Uri? spotifyUrl,
    DraftSpotlightId? id = null
  )
    : base(id ?? DraftSpotlightId.CreateUnique())
  {
    PublicId = publicId;
    DraftPublicId = draftPublicId;
    SpotlightDescription = spotlightDescription;
    SpotifyUrl = spotifyUrl;
    IsActive = false;
    IsPinned = false;
    CreatedAtUtc = DateTime.UtcNow;
  }

  private DraftSpotlight() { }

  public string PublicId { get; private set; } = default!;
  public string DraftPublicId { get; private set; } = default!;
  public string SpotlightDescription { get; private set; } = default!;
  public Uri? SpotifyUrl { get; private set; }
  public bool IsActive { get; private set; }
  public bool IsPinned { get; private set; }
  public DateTime? ActivatedAtUtc { get; private set; }
  public DateTime CreatedAtUtc { get; private set; }

  public static DraftSpotlight Create(
    string publicId,
    string draftPublicId,
    string spotlightDescription,
    Uri? spotifyUrl
  ) =>
    new(
      publicId: publicId,
      draftPublicId: draftPublicId,
      spotlightDescription: spotlightDescription,
      spotifyUrl: spotifyUrl
    );

  public void Activate()
  {
    IsActive = true;
    ActivatedAtUtc = DateTime.UtcNow;
  }

  public void Deactivate()
  {
    IsActive = false;
  }

  public void Pin()
  {
    IsPinned = true;
  }

  public void Unpin()
  {
    IsPinned = false;
  }

  public void UpdateDescription(string description)
  {
    SpotlightDescription = description;
  }

  public void UpdateSpotifyUrl(Uri? spotifyUrl)
  {
    SpotifyUrl = spotifyUrl;
  }

  internal void SetPublicId(string publicId)
  {
    ArgumentNullException.ThrowIfNull(publicId);
    PublicId = publicId;
  }
}
