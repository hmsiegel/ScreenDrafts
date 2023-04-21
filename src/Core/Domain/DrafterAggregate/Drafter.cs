namespace ScreenDrafts.Domain.DraftersAggregate;
public sealed class Drafter : AggregateRoot<DrafterId, DefaultIdType>, IAuditableEntity
{
    private readonly List<Draft> _particpatedDrafts = new();

    private Drafter(
        DrafterId drafterId,
        string? userId,
        bool hasRolloverVeto,
        bool hasRolloverVetoOverride)
        : base(drafterId ?? DrafterId.Create())
    {
        UserId = userId;
        HasRolloverVeto = hasRolloverVeto;
        HasRolloverVetoOverride = hasRolloverVetoOverride;
    }

    public string? UserId { get; private set; }
    public bool HasRolloverVeto { get; private set; } = false;
    public bool HasRolloverVetoOverride { get; private set; } = false;
    public IReadOnlyCollection<Draft> ParticipatedDrafts => _particpatedDrafts;

    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public static Drafter Create(
        string userId,
        bool hasRolloverVeto,
        bool hasRolloverVetoOverride)
    {
        return new Drafter(
            DrafterId.Create(),
            userId,
            hasRolloverVeto,
            hasRolloverVetoOverride);
    }

    public void AddParticipatedDraft(Draft draft)
    {
        _particpatedDrafts.Add(draft);
    }

    public void UpdateRolloverVeto(bool hasRolloverVeto)
    {
        HasRolloverVeto = hasRolloverVeto;
    }

    public void UpdateRolloverVetoOverride(bool hasRolloverVetoOverride)
    {
        HasRolloverVetoOverride = hasRolloverVetoOverride;
    }

    private Drafter()
    {
    }
}
