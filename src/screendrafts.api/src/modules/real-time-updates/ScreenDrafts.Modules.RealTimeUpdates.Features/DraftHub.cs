namespace ScreenDrafts.Modules.RealTimeUpdates.Features;

public sealed class DraftHub : Hub
{
  public static string GroupName(string draftPartId) => $"draft-part:{draftPartId}";

  public async Task JoinDraftPartAsync(string draftPartId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, draftPartId);
  }

  public async Task LeaveDraftPartAsync(string draftPartId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, draftPartId);
  }
}
