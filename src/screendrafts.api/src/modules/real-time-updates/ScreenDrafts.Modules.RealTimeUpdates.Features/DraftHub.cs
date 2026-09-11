namespace ScreenDrafts.Modules.RealTimeUpdates.Features;

public sealed class DraftHub : Hub
{
  public static string GroupName(string draftPartId) => $"draft-part:{draftPartId}";

  public static string HostGroupName(string draftPartId) => $"draft-part:{draftPartId}:host";

  public static string SubDraftGroupName(string draftPartId, string subDraftId) =>
    $"draft-part:{draftPartId}:sub-draft:{subDraftId}";

  // ── GuestDrafts groups ────────────────────────────────────────────────────
  // GuestDrafts has no host role, so there is no static "host group" to mirror
  // canonical's HostGroupName. Instead, reveal authority is per-pick
  // (GuestDraftPick.RevealAuthorizedParticipantId), so each participant joins
  // their OWN group once at connection time, and the PickSubmitted broadcast
  // targets whichever participant's group matches that pick's authorized
  // revealer -- see GuestDraftPickSubmittedIntegrationEventConsumer.

  public static string GuestDraftGroupName(string guestDraftId) => $"guest-draft:{guestDraftId}";

  public static string GuestDraftParticipantGroupName(string guestDraftId, string participantId) =>
    $"guest-draft:{guestDraftId}:participant:{participantId}";

  /// <summary>
  /// Flat-group-only join, mirroring JoinDraftPartAsync's shape — for
  /// contexts that only care about draft-wide broadcasts (e.g. DraftStarted,
  /// so the guest-drafts landing list can move a card from "Waiting..." to
  /// "In Progress" live) and have no participantId to join per-participant
  /// reveal routing with in the first place, since JoinGuestDraftAsync below
  /// requires one for that half.
  /// </summary>
  public async Task JoinGuestDraftFlatAsync(string guestDraftId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, GuestDraftGroupName(guestDraftId));
  }

  public async Task LeaveGuestDraftFlatAsync(string guestDraftId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, GuestDraftGroupName(guestDraftId));
  }

  /// <summary>
  /// Joins both the flat guest-draft group and this caller's own participant
  /// group. Trust is handled by JWT auth on the hub, same bar as
  /// StartCountdownAsync below -- but note the stakes here are higher: unlike
  /// the host-impersonation gap already accepted there, a client claiming a
  /// participantId that isn't theirs can read another player's pending pick
  /// (movie title) before it's revealed, which breaks the core draft mechanic,
  /// not just a UI permission. Validating participantId against the JWT-resolved
  /// caller identity (via IUsersApi) before joining is a flagged follow-up, not
  /// done here.
  /// </summary>
  public async Task JoinGuestDraftAsync(string guestDraftId, string participantId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, GuestDraftGroupName(guestDraftId));
    await Groups.AddToGroupAsync(
      Context.ConnectionId,
      GuestDraftParticipantGroupName(guestDraftId, participantId)
    );
  }

  public async Task LeaveGuestDraftAsync(string guestDraftId, string participantId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, GuestDraftGroupName(guestDraftId));
    await Groups.RemoveFromGroupAsync(
      Context.ConnectionId,
      GuestDraftParticipantGroupName(guestDraftId, participantId)
    );
  }

  public async Task JoinDraftPartAsync(string draftPartId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(draftPartId));
  }

  public async Task LeaveDraftPartAsync(string draftPartId)
  {
    await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(draftPartId));
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

  public async Task JoinSubDraftGroupAsync(string draftPartId, string subDraftId)
  {
    await Groups.AddToGroupAsync(Context.ConnectionId, SubDraftGroupName(draftPartId, subDraftId));
  }

  public async Task LeaveSubDraftGroupAsync(string draftPartId, string subDraftId)
  {
    await Groups.RemoveFromGroupAsync(
      Context.ConnectionId,
      SubDraftGroupName(draftPartId, subDraftId)
    );
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
