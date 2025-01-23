namespace ScreenDrafts.Modules.Drafts.Contracts;

public sealed record DraftResponse(
  Guid Id,
  string Title,
  string DraftType,
  int NumberOfDrafters,
  int NumberOfCommissioners,
  int NumberOfMovies)
{
  public DraftResponse() : this(
    Guid.Empty,
    string.Empty,
    string.Empty,
    default,
    default,
    default)
  {
  }
}
