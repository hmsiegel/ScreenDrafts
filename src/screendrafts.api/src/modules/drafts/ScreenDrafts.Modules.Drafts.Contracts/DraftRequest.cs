namespace ScreenDrafts.Modules.Drafts.Contracts;

public sealed record DraftRequest(
  string Title,
  int DraftType,
  int NumberOfDrafters,
  int NumberOfCommissioners,
  int NumberOfMovies);

