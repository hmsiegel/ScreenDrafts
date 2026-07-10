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

  /// <summary>
  /// True when the caller is the contestant or the
  /// designated submitter for a predictor slot on this part.
  /// </summary>
  public bool IsPredictor { get; init; }

  /// <summary>
  /// The contestant slot the caller acts under.
  /// Required by SubmitPredictionSet. Null when IsPredictor is false.
  /// </summary>
  public string? ContestantPublicId { get; init; }

  /// <summary>
  /// True when the contestant already has a set for this part.
  /// SubmitPredictionSet rejecst a second one - the UI needs to know first.
  /// </summary>
  public bool HasSubmittedPrediction { get; init; }
}
