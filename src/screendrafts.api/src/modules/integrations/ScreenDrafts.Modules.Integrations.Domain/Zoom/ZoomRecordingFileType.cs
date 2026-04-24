namespace ScreenDrafts.Modules.Integrations.Domain.Zoom;

public sealed class ZoomRecordingFileType(string name, int value)
  : SmartEnum<ZoomRecordingFileType>(name, value)
{
  public static readonly ZoomRecordingFileType Video = new("MP4", 0);
  public static readonly ZoomRecordingFileType Audio = new("M4A", 1);
  public static readonly ZoomRecordingFileType Transcript = new("TRANSCRIPT", 2);
  public static readonly ZoomRecordingFileType Chat = new("CHAT", 3);
  public static readonly ZoomRecordingFileType Timeline = new("TIMELINE", 4);
  public static readonly ZoomRecordingFileType ClosedCaption = new("CC", 5);
}
