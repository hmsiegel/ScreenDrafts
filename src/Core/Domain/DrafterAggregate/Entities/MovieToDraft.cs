namespace ScreenDrafts.Domain.DrafterAggregate.Entities;
public sealed class MovieToDraft : Entity<MovieToDraftId>
{
    private MovieToDraft(
        MovieToDraftId id,
        MovieId? movieId,
        DraftId? draftId)
        : base(id ?? MovieToDraftId.CreateUnique())
    {
        MovieId = movieId;
        DraftId = draftId;
    }

    public DraftId? DraftId { get; private set; }
    public MovieId? MovieId { get; private set; }

    public static MovieToDraft Create(
        MovieId movieId,
        DraftId draftId)
    {
        return new MovieToDraft(
            MovieToDraftId.CreateUnique(),
            movieId,
            draftId);
    }

    private MovieToDraft()
    {
    }
}
