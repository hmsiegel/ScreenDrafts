namespace ScreenDrafts.Common.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
  public DateTime UtcNow => DateTime.UtcNow;

  public DateTimeOffset UtcTimeZoneNow => DateTimeOffset.UtcNow;
}
