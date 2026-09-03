namespace ScreenDrafts.Modules.RealTimeUpdates.IntegrationTests.Doubles;

internal sealed record LoggedEntry(LogLevel Level, EventId EventId, string Message);

/// <summary>
/// Hand-rolled ILogger test double -- no mocking library is referenced anywhere in
/// the solution. NullLogger&lt;T&gt; (used by every other consumer test in this
/// project) discards everything, so it can't back an assertion that a specific
/// warning was actually logged; this records every call instead.
/// </summary>
internal sealed class RecordingLogger<T> : ILogger<T>
{
  private readonly List<LoggedEntry> _entries = [];

  public IReadOnlyList<LoggedEntry> Entries => _entries;

  public IDisposable? BeginScope<TState>(TState state)
    where TState : notnull => null;

  public bool IsEnabled(LogLevel logLevel) => true;

  public void Log<TState>(
    LogLevel logLevel,
    EventId eventId,
    TState state,
    Exception? exception,
    Func<TState, Exception?, string> formatter
  )
  {
    ArgumentNullException.ThrowIfNull(formatter);
    _entries.Add(new LoggedEntry(logLevel, eventId, formatter(state, exception)));
  }
}
