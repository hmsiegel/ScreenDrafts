namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

/// <summary>
/// Captures SignalR messages sent to DraftHub during integration tests.
/// The current RealTimeUpdates consumers send a single event name "PickListUpdated"
/// for all pick-related actions. Connect this client before Step 6 (StartDraft) and
/// read <see cref="ReceivedMessages"/> after the action under test.
///
/// NOTE: Requires the test WebApplicationFactory to have SignalR mapped.
/// Use <see cref="ConnectAsync"/> before the scenario starts and
/// <see cref="DisposeAsync"/> during test teardown.
/// </summary>
public sealed class SignalRTestClient : IAsyncDisposable
{
  private readonly List<SignalRMessage> _messages = [];
  private readonly SemaphoreSlim _lock = new(1, 1);

  // Placeholder for HubConnection — add Microsoft.AspNetCore.SignalR.Client reference
  // and replace with real HubConnection once the package is in Directory.Packages.props.

  public IReadOnlyList<SignalRMessage> ReceivedMessages
  {
    get
    {
      _lock.Wait();
      try { return _messages.ToList().AsReadOnly(); }
      finally { _lock.Release(); }
    }
  }

  /// <summary>Connect to the test server's DraftHub and join a draft-part group.</summary>
  public static Task ConnectAsync(Uri hubUrl, string draftPartPublicId, CancellationToken cancellationToken = default)
  {
    // Stub implementation — wire real HubConnection here when SignalR client package added.
    _ = draftPartPublicId;
    _ = hubUrl;
    _ = cancellationToken;
    return Task.CompletedTask;
  }

  /// <summary>
  /// Wait until at least one message with the given method name arrives,
  /// or the timeout elapses. Returns the first matching message.
  /// </summary>
  public async Task<SignalRMessage?> WaitForMessageAsync(
    string method,
    TimeSpan? timeout = null,
    CancellationToken cancellationToken = default)
  {
    var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(5));
    while (DateTime.UtcNow < deadline)
    {
      await Task.Delay(50, cancellationToken);
      var msg = ReceivedMessages.FirstOrDefault(m => m.Method == method);
      if (msg is not null) return msg;
    }

    return null;
  }

  public void Clear()
  {
    _lock.Wait();
    try { _messages.Clear(); }
    finally { _lock.Release(); }
  }

  public ValueTask DisposeAsync()
  {
    _lock.Dispose();
    return ValueTask.CompletedTask;
  }
}

/// <summary>A single hub message captured by <see cref="SignalRTestClient"/>.</summary>
public sealed record SignalRMessage(string Method, object?[] Args);
