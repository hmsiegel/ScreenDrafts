namespace ScreenDrafts.Domain.MovieAggregate;
public sealed class Movie : AggregateRoot<MovieId, DefaultIdType>, IAuditableEntity
{
    private readonly List<string?> _writers = new();
    private readonly List<Draft> _draftsSelectedIn = new();
    private readonly List<Draft> _draftsVetoedIn = new();

    private Movie(
        MovieId movieId,
        string? title,
        string? year,
        string? director,
        string? imageUrl,
        string? imdbUrl)
        : base(movieId ?? MovieId.Create())
    {
        Title = title;
        Year = year;
        Director = director;
        ImageUrl = imageUrl;
        ImdbUrl = imdbUrl;
    }

    public string? Title { get; private set; }
    public string? Year { get; private set; }
    public string? Director { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? ImdbUrl { get; private set; }
    public bool IsInMarqueeOfFame { get; private set; } = false;
    public IReadOnlyCollection<string?> Writers => _writers;
    public IReadOnlyCollection<Draft> DraftsSelectedIn => _draftsSelectedIn;
    public IReadOnlyCollection<Draft> DraftsVetoedIn => _draftsVetoedIn;

    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public static Movie Create(
        string title,
        string year,
        string director,
        string imageUrl,
        string imdbUrl)
    {
        return new Movie(
            MovieId.Create(),
            title,
            year,
            director,
            imageUrl,
            imdbUrl);
    }

    public void AddWriter(string writer)
    {
        _writers.Add(writer);
    }

    public void AddDraftSelectedIn(Draft draft)
    {
        _draftsSelectedIn.Add(draft);
    }

    public void AddDraftVetoedIn(Draft draft)
    {
        _draftsVetoedIn.Add(draft);
    }

    private Movie()
    {

    }
}
