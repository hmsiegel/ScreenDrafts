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

  /// <summary>
  /// Called by a co-host client. Validates the caller is connected (trust is
  /// handled by JWT auth on the hub), then relays CountdownStarted to the group.
  /// Full host validation (is this connection actually a host for this part?)
  /// is a hardening concern deferred to a follow-up.
  /// </summary>
  public async Task StartCountdownAsync(string draftPartPublicId, string targetParticipantId)
  {
    await Clients
      .Group(GroupName(draftPartPublicId))
      .SendAsync(
        "CountdownStarted",
        new { DraftPartPublicId = draftPartPublicId, TargetParticipantId = targetParticipantId }
      );
  }
}
