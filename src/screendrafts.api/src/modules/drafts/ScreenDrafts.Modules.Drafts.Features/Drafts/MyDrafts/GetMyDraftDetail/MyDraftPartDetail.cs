namespace ScreenDrafts.Modules.Drafts.Features.Drafts.MyDrafts.GetMyDraftDetail;

internal sealed record MyDraftPartDetail
{
  public string DraftPartPublicId { get; init; } = default!;
  public int PartIndex { get; init; }
  public int Status { get; init; }
  public bool IsHost { get; init; }
  public bool IsDrafter { get; init; }
  public string? AttendanceStatus { get; init; }
  public DateOnly? ReleaseDate { get; init; }
}
