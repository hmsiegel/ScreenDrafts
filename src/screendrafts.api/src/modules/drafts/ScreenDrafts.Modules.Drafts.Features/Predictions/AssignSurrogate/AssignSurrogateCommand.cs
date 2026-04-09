namespace ScreenDrafts.Modules.Drafts.Features.Predictions.AssignSurrogate;

internal sealed record AssignSurrogateCommand : ICommand
{
  public required string DraftPartPublicId { get; init; }
  public required string PrimarySetPublicId { get; init; }
  public required string SurrogateSetPublicId { get; init; }
  public required int MergePolicy { get; init; }
}
