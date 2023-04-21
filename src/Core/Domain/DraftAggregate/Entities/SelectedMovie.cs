namespace ScreenDrafts.Domain.DraftAggregate.Entities;

public sealed class SelectedMovie : Entity<SelectedMovieId>
{
    private SelectedMovie(
        SelectedMovieId id,
        MovieId? movieId,
        int draftPosition,
        DrafterId? drafterId)
        : base(id ?? SelectedMovieId.CreateUnique())
    {
        MovieId = movieId;
        DraftPosition = draftPosition;
        DrafterId = drafterId;
    }

    public MovieId? MovieId { get; private set; }
    public int DraftPosition { get; private set; }
    public DrafterId? DrafterId { get; private set; }
    public bool IsVetoed { get; private set; } = false;
    public DrafterId? DrafterWhoPlayedVeto { get; private set; } = null;
    public bool WasVetoOverride { get; private set; } = false;
    public DrafterId? DrafterWhoPlayedVetoOverride { get; private set; } = null;
    public bool WasCommissonerOverride { get; private set; } = false;

    public static SelectedMovie Create(
        MovieId movieId,
        int draftPosition,
        DrafterId drafterId)
    {
        return new SelectedMovie(
            SelectedMovieId.CreateUnique(),
            movieId,
            draftPosition,
            drafterId);
    }

    public void Veto(DrafterId drafterId)
    {
        IsVetoed = true;
        DrafterWhoPlayedVeto = drafterId;
    }

    public void VetoOverride(DrafterId drafterId)
    {
        WasVetoOverride = true;
        DrafterWhoPlayedVetoOverride = drafterId;
    }

    public void CommissionerOverride()
    {
        WasCommissonerOverride = true;
    }

    private SelectedMovie()
    {
    }
}