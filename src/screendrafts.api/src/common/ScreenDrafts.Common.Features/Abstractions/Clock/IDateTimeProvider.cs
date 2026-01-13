namespace ScreenDrafts.Common.Features.Abstractions.Clock;

public interface IDateTimeProvider
{
  public DateTime UtcNow { get; }
}
