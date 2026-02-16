namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafters.Queries.GetDrafterProfile;

public sealed record VetoHistoryItem(
  DraftBrief Draft,
  Guid VetoId,
  Guid TargetPickId,
  int Position,
  int PlayOrder,
  Guid MovieId,
  string MovieTitle,
  Guid TargetDrafterId,
  string TargetDrafterDisplayName,
  bool WasVetoOverride,
  Guid? OverrideById,
  string? OverrideByName);
