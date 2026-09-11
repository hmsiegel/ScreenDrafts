using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Abstractions;

public abstract class GuestDraftsBaseTest : BaseTest
{
  protected static Draft CreateGuestDraft(
    DraftType? guestDraftType = null,
    Guid? ownerUserId = null,
    string? title = null
  ) => GuestDraftScenarioFactory.CreateGuestDraft(guestDraftType, ownerUserId, title);

  protected static DraftParticipant AddParticipant(Draft guestDraft, bool isOwner = false) =>
    GuestDraftScenarioFactory.AddParticipant(guestDraft, isOwner);

  protected static string GeneratePositionPublicId(string name) =>
    GuestDraftScenarioFactory.GeneratePositionPublicId(name);

  protected static (
    Draft GuestDraft,
    DraftParticipant Owner,
    DraftParticipant Other
  ) CreateInProgressStandardGuestDraft() =>
    GuestDraftScenarioFactory.CreateInProgressStandardGuestDraft();

  protected static (
    Draft GuestDraft,
    IReadOnlyList<DraftParticipant> Participants,
    IReadOnlyList<DraftPosition> Positions
  ) CreateInProgressCustomGuestDraft(int participantCount, DraftType? guestDraftType = null) =>
    GuestDraftScenarioFactory.CreateInProgressCustomGuestDraft(participantCount, guestDraftType);

  protected static Pick CreatePick(
    Draft guestDraft,
    DraftParticipant playedBy,
    int position = 1,
    int playOrder = 1,
    string? moviePublicId = null
  ) => GuestDraftPickFactory.CreatePick(guestDraft, playedBy, position, playOrder, moviePublicId);
}
