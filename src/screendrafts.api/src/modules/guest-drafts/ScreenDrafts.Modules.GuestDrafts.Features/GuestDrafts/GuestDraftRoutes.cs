namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts;

internal static class GuestDraftsRoutes
{
  internal const string Base = "/guest-drafts";
  internal const string ById = Base + "/{publicId}";
  internal const string Participants = ById + "/participants";
  internal const string GuestDraftStatus = ById + "/status";
  internal const string FixedBoardLayout = ById + "/board/fixed-layout";
  internal const string CustomBoardLayout = ById + "/board/custom-layout";
  internal const string PositionAssign = ById + "/positions/{positionPublicId}/assign";

  internal const string Picks = ById + "/picks";
  internal const string PickByPlayOrder = ById + "/picks/{playOrder}";
  internal const string PickVeto = PickByPlayOrder + "/veto";
  internal const string PickVetoOverride = PickByPlayOrder + "/veto-override";
  internal const string PickCommissionerOverride = PickByPlayOrder + "/commissioner-override";
  internal const string PickUndoVeto = PickByPlayOrder + "/undo-veto";
  internal const string PickReveal = PickByPlayOrder + "/reveal";
}
