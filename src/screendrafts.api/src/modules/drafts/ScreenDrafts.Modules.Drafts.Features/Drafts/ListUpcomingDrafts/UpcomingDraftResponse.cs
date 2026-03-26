namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListUpcomingDrafts;

internal sealed record UpcomingDraftResponse
{
  public string DraftPartPublicId { get; init; } = string.Empty;
  public string DraftPublicId { get; init; } = string.Empty;
  public string Title { get; init; } = string.Empty;
  public int PartNumber { get; init; }
  public DateOnly? ReleaseDate { get; init; }

  public DraftStatus Status { get; init; } = default!;

  public DraftUserCapabilities Capabilities { get; private set; } = new(null, false, false, false, false, false);

  public void SetCapabilities(DraftUserCapabilities capabilities)
  {
    Capabilities = capabilities;
  }
}
