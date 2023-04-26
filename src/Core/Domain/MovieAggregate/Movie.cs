namespace ScreenDrafts.Domain.MovieAggregate;
public sealed class Movie : AuditableEntity, IAggregateRoot
{
    private readonly List<string?> _writers = new();
    private readonly List<DraftId> _draftsSelectedIn = new();
    private readonly List<DraftId> _draftsVetoedIn = new();

    private Movie(
        string? title,
        string? year,
        string? director,
        string? imageUrl,
        string? imdbUrl)
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
    public IReadOnlyCollection<DraftId> DraftsSelectedIn => _draftsSelectedIn.AsReadOnly();
    public IReadOnlyCollection<DraftId> DraftsVetoedIn => _draftsVetoedIn.AsReadOnly();

    public static Movie Create(
        string title,
        string year,
        string director,
        string imageUrl,
        string imdbUrl)
    {
        return new Movie(
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

    public void AddDraftSelectedIn(DraftId draftId)
    {
        _draftsSelectedIn.Add(draftId);
    }

    public void AddDraftVetoedIn(DraftId draftId)
    {
        _draftsVetoedIn.Add(draftId);
    }

    private Movie()
    {
    }
}
