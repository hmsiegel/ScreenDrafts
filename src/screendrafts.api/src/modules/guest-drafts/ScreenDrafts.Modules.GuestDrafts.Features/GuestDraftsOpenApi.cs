namespace ScreenDrafts.Modules.GuestDrafts.Features;

internal static class GuestDraftsOpenApi
{
  public static class Tags
  {
    public const string GuestDrafts = "GuestDrafts";
    public const string GuestDrafters = "GuestDrafters";
  }

  public static class Names
  {
    // Guest Drafts
    public const string GuestDrafts_CreateGuestDraft = "GuestDrafts.CreateGuestDraft";
    public const string GuestDrafts_AddParticipant = "GuestDrafts.AddParticipant";
    public const string GuestDrafts_AssignParticipantToPosition =
      "GuestDrafts.AssignParticipantToPosition";
    public const string GuestDrafts_SetStatus = "GuestDrafts.SetStatus";
    public const string GuestDrafts_PlayPick = "GuestDrafts.PlayPick";
    public const string GuestDrafts_UndoPick = "GuestDrafts.UndoPick";
    public const string GuestDrafts_ApplyVeto = "GuestDrafts.ApplyVeto";
    public const string GuestDrafts_ApplyVetoOverride = "GuestDrafts.ApplyVetoOverride";
    public const string GuestDrafts_ApplyCommissionerOverride =
      "GuestDrafts.ApplyCommissionerOverride";
    public const string GuestDrafts_UndoVeto = "GuestDrafts.UndoVeto";
    public const string GuestDrafts_RevealPick = "GuestDrafts.RevealPick";
    public const string GuestDrafts_GetGameplay = "GuestDrafts.GetGameplay";
    public const string GuestDrafts_UpdateGuestDraft = "GuestDrafts.UpdateGuestDraft";
    public const string GuestDrafters_Search = "GuestDrafters.Search";
    public const string GuestDrafts_Search = "GuestDrafts.Search";
    public const string GuestDrafts_GetDetails = "GuestDrafts.GetDetails";
  }
}
