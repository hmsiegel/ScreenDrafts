namespace ScreenDrafts.Modules.Reporting.Domain.Movies;

public sealed class MovieCanonicalPick : Entity<MovieCanonicalPickId>
{
  private MovieCanonicalPick(
    string moviePublicId,
    string draftPartPublicId,
    int boardPosition,
    DateTime pickedAt,
    MovieCanonicalPickId? id = null)
    : base(id ?? MovieCanonicalPickId.CreateUnique())
  {
    MoviePublicId = moviePublicId;
    DraftPartPublicId = draftPartPublicId;
    BoardPosition = boardPosition;
    PickedAt = pickedAt;
  }

  private MovieCanonicalPick() { }

  public string MoviePublicId { get; private set; } = default!;
  public string DraftPartPublicId { get; private set; } = default!;
  public int BoardPosition { get; private set; }
  public DateTime PickedAt { get; private set; }

  public static MovieCanonicalPick Create(
    string moviePublicId,
    string draftPartPublicId,
    int boardPosition)
  {
    return new MovieCanonicalPick(
      moviePublicId: moviePublicId,
      draftPartPublicId: draftPartPublicId,
      boardPosition: boardPosition,
      pickedAt: DateTime.UtcNow);
  }
}
