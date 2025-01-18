namespace ScreenDrafts.Modules.Drafts.Domain.Drafts;

public sealed record DraftRequest(
  string Title,
  int DraftType,
  int NumberOfDrafters,
  int NumberOfCommissioners,
  int NumberOfMovies);

