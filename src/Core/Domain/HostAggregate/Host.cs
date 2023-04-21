namespace ScreenDrafts.Domain.CommissionerAggregate;
public sealed class Host : AggregateRoot<HostId, DefaultIdType>, IAuditableEntity
{
    private Host(HostId id, string? userId)
        : base(id ?? HostId.Create())
    {
        UserId = userId;
    }

    public string? UserId { get; private set; }
    public int PredictionPoints { get; private set; }

    public DefaultIdType CreatedBy { get; set; }
    public DateTime CreatedOn { get; }
    public DefaultIdType LastModifiedBy { get; set; }
    public DateTime? LastModifiedOn { get; set; }

    public static Host Create(string? userId) => new(HostId.Create(), userId);

    public void AddPredictionPoints(int points) => PredictionPoints += points;

    private Host()
    {
    }
}
