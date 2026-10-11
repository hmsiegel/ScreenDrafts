namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Abstractions;

/// <summary>
/// IDateTimeProvider whose clock tests can pin and advance. Until <see cref="Pin"/> is
/// called it follows the real clock, so registering it app-wide changes nothing for tests
/// that never touch it. <see cref="Reset"/> returns it to the real clock.
/// </summary>
public sealed class MutableDateTimeProvider : IDateTimeProvider
{
  private DateTime? _pinned;

  public DateTime UtcNow => _pinned ?? DateTime.UtcNow;

  public DateTimeOffset UtcTimeZoneNow => new(UtcNow, TimeSpan.Zero);

  public MutableDateTimeProvider() { }

  public MutableDateTimeProvider(DateTime pinnedUtcNow) => Pin(pinnedUtcNow);

  public void Pin(DateTime utcNow) => _pinned = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);

  public void Advance(TimeSpan by) => Pin(UtcNow + by);

  public void Reset() => _pinned = null;
}
