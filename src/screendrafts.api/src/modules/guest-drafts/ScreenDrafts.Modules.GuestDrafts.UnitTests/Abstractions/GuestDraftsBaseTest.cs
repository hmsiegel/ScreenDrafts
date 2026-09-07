namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Abstractions;

public abstract class GuestDraftsBaseTest : BaseTest
{
  protected static GuestDraft CreateGuestDraft(
    GuestDraftType? guestDraftType = null,
    Guid? ownerUserId = null,
    string? title = null) =>
    GuestDraftScenarioFactory.CreateGuestDraft(guestDraftType, ownerUserId, title);

  protected static GuestDraftParticipant AddParticipant(GuestDraft guestDraft, bool isOwner = false) =>
    GuestDraftScenarioFactory.AddParticipant(guestDraft, isOwner);

  protected static string GeneratePositionPublicId(string name) =>
    GuestDraftScenarioFactory.GeneratePositionPublicId(name);

  protected static (GuestDraft GuestDraft, GuestDraftParticipant Owner, GuestDraftParticipant Other)
    CreateInProgressStandardGuestDraft() =>
    GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();

  protected static (GuestDraft GuestDraft, IReadOnlyList<GuestDraftParticipant> Participants, IReadOnlyList<GuestDraftPosition> Positions)
    CreateInProgressCustomGuestDraft(int participantCount, GuestDraftType? guestDraftType = null) =>
    GuestDraftScenarioFactory.CreateInProgressCustomGuestDraft(participantCount, guestDraftType);

  protected static GuestDraftPick CreatePick(
    GuestDraft guestDraft,
    GuestDraftParticipant playedBy,
    int position = 1,
    int playOrder = 1,
    string? moviePublicId = null) =>
    GuestDraftPickFactory.CreatePick(guestDraft, playedBy, position, playOrder, moviePublicId);
}
