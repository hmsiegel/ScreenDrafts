namespace ScreenDrafts.Domain.DraftAggregate;
public sealed class Draft : AuditableEntity, IAggregateRoot
{
    private readonly List<SelectedMovie> _selectedMovies = new();
    private readonly List<HostId> _hosts = new();
    private readonly List<DrafterId> _drafterIds = new();

    private Draft(
        string? draftName,
        DraftType? draftType,
        int numberOfDrafters)
    {
        DraftName = draftName;
        DraftType = draftType;
        NumberOfDrafters = numberOfDrafters;
    }

    public string? DraftName { get; private set; }
    public DraftType? DraftType { get; private set; }
    public int NumberOfDrafters { get; private set; }
    public DateOnly EpisodeReleaseDate { get; private set; }
    public int RuntimeInMinutes { get; private set; }
    public int EpisodeNumber { get; private set; }
    public IReadOnlyList<SelectedMovie> SelectedMovies => _selectedMovies.AsReadOnly();
    public IReadOnlyList<HostId> Hosts => _hosts.AsReadOnly();
    public IReadOnlyList<DrafterId> DrafterIds => _drafterIds.AsReadOnly();

    private Draft()
    {
    }

    public static Draft Create(
        string draftName,
        DraftType draftType,
        int numberOfDrafters)
    {
        if (draftType != DraftType.Regular)
        {
            numberOfDrafters = 2;
        }

        return new Draft(
            draftName,
            draftType,
            numberOfDrafters);
    }

    public void AddDraftedMovie(SelectedMovie movie)
    {
        _selectedMovies.Add(movie);
    }

    public void AddHost(HostId hostId)
    {
        _hosts.Add(hostId);
    }

    public void AddDrafter(DrafterId drafterId)
    {
        _drafterIds.Add(drafterId);
    }

    public void AddEpisodeReleaseDate(DateOnly episodeReleaseDate)
    {
        EpisodeReleaseDate = episodeReleaseDate;
    }

    public void AddRuntimeInMinutes(int runtimeInMinutes)
    {
        RuntimeInMinutes = runtimeInMinutes;
    }

    public void AddEpisodeNumber(int episodeNumber)
    {
        EpisodeNumber = episodeNumber;
    }
}
