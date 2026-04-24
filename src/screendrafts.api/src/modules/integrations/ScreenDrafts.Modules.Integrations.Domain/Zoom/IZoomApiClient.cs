namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public interface IZoomApiClient : IDisposable
{
  Task StartRecordingAsync(string sessionName, CancellationToken cancellationToken = default);
  Task StopRecordingAsync(string sessionName, CancellationToken cancellationToken = default);
}
