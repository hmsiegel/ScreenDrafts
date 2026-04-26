namespace ScreenDrafts.Modules.RealTimeUpdates.Features;

public sealed class DraftHub : Hub
{
  public static string GroupName(string draftPartId) => $"draft-part:{draftPartId}";
  public static string HostGroupName(string draftPartId) => $"draft-part:{draftPartId}:host";

  public async Task JoinDraftPartAsync(string draftPartId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, draftPartId);
  }

  public async Task LeaveDraftPartAsync(string draftPartId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, draftPartId);
  }

  public async Task JoinDraftPartAsHostAsync(string draftPartId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(draftPartId));
    await Groups.AddToGroupAsync(Context.ConnectionId, HostGroupName(draftPartId));
  }

  public async Task LeaveDraftPartAsHostAsync(string draftPartId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(draftPartId));
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, HostGroupName(draftPartId));
  }
}
