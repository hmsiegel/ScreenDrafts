using ScreenDrafts.Common.Application.Clock;

namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// Hand-rolled IDateTimeProvider test double -- no mocking library is referenced
/// anywhere in the solution. Fixes UtcNow/UtcTimeZoneNow to a known value so
/// domain-event-handler tests can assert a published integration event's
/// OccurredOnUtc came from this provider, not DateTime.UtcNow directly.
/// </summary>
public sealed class FakeDateTimeProvider(DateTime utcNow) : IDateTimeProvider
{
  public DateTime UtcNow { get; } = utcNow;

  public DateTimeOffset UtcTimeZoneNow { get; } = utcNow;
}
