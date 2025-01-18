namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed record DraftResponse(
  Guid Id,
  string Title,
  DraftType DraftType,
  int NumberOfDrafters,
  int NumberOfCommissioners,
  int NumberOfMovier);
