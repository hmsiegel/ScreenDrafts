namespace ScreenDrafts.Domain.CommissionerAggregate;
public sealed class Host : AuditableEntity, IAggregateRoot
{
    private Host(
        UserId userId)
    {
        UserId = userId;
    }

    public UserId UserId { get; private set; }
    public int PredictionPoints { get; private set; }

    public static Host Create(UserId userId) => new(userId);

    public void AddPredictionPoints(int points) => PredictionPoints += points;

    public void ResetPredictionPoints() => PredictionPoints = 0;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Host()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }
}
