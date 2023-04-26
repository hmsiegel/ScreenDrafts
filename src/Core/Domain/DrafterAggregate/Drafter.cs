using Microsoft.EntityFrameworkCore;

namespace ScreenDrafts.Domain.DraftersAggregate;
public sealed class Drafter : AuditableEntity, IAggregateRoot
{
    private readonly List<DraftId> _particpatedDrafts = new();
    private readonly List<MovieToDraft> _moviesToDraft = new();

    private Drafter(
        UserId? userId,
        bool hasRolloverVeto,
        bool hasRolloverVetoOverride)
    {
        UserId = userId;
        HasRolloverVeto = hasRolloverVeto;
        HasRolloverVetoOverride = hasRolloverVetoOverride;
    }

    public UserId? UserId { get; private set; }
    public bool HasRolloverVeto { get; private set; }
    public bool HasRolloverVetoOverride { get; private set; }

    [BackingField(nameof(_particpatedDrafts))]
    public IReadOnlyList<DraftId> ParticipatedDrafts => _particpatedDrafts.AsReadOnly();
    public IReadOnlyList<MovieToDraft> MoviesToDraft => _moviesToDraft.AsReadOnly();

    public static Drafter Create(
        UserId? userId,
        bool hasRolloverVeto,
        bool hasRolloverVetoOverride)
    {
        return new Drafter(
            userId,
            hasRolloverVeto,
            hasRolloverVetoOverride);
    }

    public void AddParticipatedDraft(DraftId draftId)
    {
        _particpatedDrafts.Add(draftId);
    }

    public void UpdateRolloverVeto(bool hasRolloverVeto)
    {
        HasRolloverVeto = hasRolloverVeto;
    }

    public void UpdateRolloverVetoOverride(bool hasRolloverVetoOverride)
    {
        HasRolloverVetoOverride = hasRolloverVetoOverride;
    }

    public void AddMovieToDraft(MovieId movieId, DraftId draftId)
    {
        _moviesToDraft.Add(MovieToDraft.Create(movieId, draftId));
    }

    private Drafter()
    {
    }
}
