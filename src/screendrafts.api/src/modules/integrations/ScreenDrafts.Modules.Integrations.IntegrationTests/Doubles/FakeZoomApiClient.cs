namespace ScreenDrafts.Modules.Integrations.IntegrationTests.Doubles;

/// <summary>
/// Records calls to <see cref="IZoomApiClient"/> so that consumer tests can
/// verify the correct API method was called with the correct session name.
/// </summary>
internal sealed class FakeZoomApiClient : IZoomApiClient
{
  public List<string> StartedSessions { get; } = [];
  public List<string> StoppedSessions { get; } = [];

  public Task StartRecordingAsync(string sessionName, CancellationToken cancellationToken = default)
  {
    StartedSessions.Add(sessionName);
    return Task.CompletedTask;
  }

  public Task StopRecordingAsync(string sessionName, CancellationToken cancellationToken = default)
  {
    StoppedSessions.Add(sessionName);
    return Task.CompletedTask;
  }

  public void Dispose() { }
}
