namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

internal sealed record SetReleaseDateRequest
{
  [FromRoute(Name = "draftPartId")]
  public string DraftPartId { get; set; } = default!;
  public DateOnly ReleaseDate { get; set; }
  public int ReleaseChannel { get; set; }
}
