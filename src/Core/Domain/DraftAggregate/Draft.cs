namespace ScreenDrafts.Domain.DraftAggregate;
public sealed class Draft : AggregateRoot<DraftId, DefaultIdType>, IAuditableEntity
{
    private readonly List<SelectedMovie> _selectedMovies = new();
    private readonly List<Host> _hosts = new();

    private Draft(
        DraftId draftId,
        string? draftName,
        DraftType? draftType,
        int episodeNumber)
        : base(draftId ?? DraftId.Create())
    {
        DraftName = draftName;
        DraftType = draftType;
        EpisodeNumber = episodeNumber;
    }

    public string? DraftName { get; private set; }
    public DraftType? DraftType { get; private set; }
    public DateOnly EpisodeReleaseDate { get; private set; }
    public int RuntimeInMinutes { get; private set; }
    public int EpisodeNumber { get; private set; }
    public IReadOnlyCollection<SelectedMovie> SelectedMovies => _selectedMovies;
    public IReadOnlyCollection<Host> Hosts => _hosts;

    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    private Draft()
    {
    }

    public static Draft Create(
        string draftName,
        DraftType draftType,
        int episodeNumber)
    {
        return new Draft(
            DraftId.Create(),
            draftName,
            draftType,
            episodeNumber);
    }

    public void AddDraftedMovie(SelectedMovie movie)
    {
        _selectedMovies.Add(movie);
    }

    public void AddHost(Host host)
    {
        _hosts.Add(host);
    }
}
