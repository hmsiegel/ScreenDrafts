namespace ScreenDrafts.Modules.GuestDrafts.Features;

internal static class GuestDraftsAuth
{
  internal static class Roles
  {
    internal const string Admin = "Administrator";
    internal const string SuperAdmin = "SuperAdministrator";
    internal const string Guest = "Guest";
  }

  internal static class Permissions
  {
    // Guest Drafts
    internal const string GuestDraftCreate = "guest-drafts:create";
    internal const string GuestDraftRead = "guest-drafts:read";
    internal const string GuestDraftInviteParticipant = "guest-drafts:invite-participant";
    internal const string GuestDraftSetBoard = "guest-drafts:set-board";
    internal const string GuestDraftAssignPosition = "guest-drafts:assign-position";
    internal const string GuestDraftSetStatus = "guest-drafts:set-status";
    internal const string GuestDraftPlayPick = "guest-drafts:play-pick";
    internal const string GuestDraftUndoPick = "guest-drafts:undo-pick";
    internal const string GuestDraftApplyVeto = "guest-drafts:apply-veto";
    internal const string GuestDraftApplyVetoOverride = "guest-drafts:apply-veto-override";
    internal const string GuestDraftApplyCommissionerOverride =
      "guest-drafts:apply-commissioner-override";
    internal const string GuestDraftUndoVeto = "guest-drafts:undo-veto";
    internal const string GuestDraftRevealPick = "guest-drafts:reveal-pick";
  }
}
