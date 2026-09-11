using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Enums;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.TestUtils;

/// <summary>
/// Builds GuestDraft aggregates in various lifecycle states for tests. State
/// transitions are driven exclusively through GuestDraft's public API -- there is no
/// other way to reach them, since GuestDraftParticipant/GuestDraftPick/GuestDraftGameBoard
/// construction is internal to the aggregate.
/// </summary>
public static class GuestDraftScenarioFactory
{
  private static readonly Faker _faker = new();

  public static Draft CreateGuestDraft(
    DraftType? guestDraftType = null,
    Guid? ownerUserId = null,
    string? title = null
  )
  {
    return Draft
      .Create(
        publicId: _faker.Random.AlphaNumeric(10),
        ownerUserId: ownerUserId ?? Guid.NewGuid(),
        title: title ?? _faker.Lorem.Sentence(3),
        guestDraftType: guestDraftType ?? DraftType.Standard
      )
      .Value;
  }

  /// <summary>
  /// Adds a participant referencing a freshly-minted GuestDrafter id. GuestDraft
  /// itself never validates that this id corresponds to a real, persisted
  /// GuestDrafter -- that cross-aggregate check lives in
  /// AddParticipantCommandHandler, not here -- so a raw Guid stands in fine for
  /// domain-level tests.
  /// </summary>
  public static DraftParticipant AddParticipant(
    Draft guestDraft,
    bool isOwner = false,
    Guid? guestDrafterId = null
  )
  {
    ArgumentNullException.ThrowIfNull(guestDraft);

    var participant = Participant.From(DrafterId.Create(guestDrafterId ?? Guid.NewGuid()));
    return guestDraft.AddParticipant(participant, isOwner).Value;
  }

  public static string GeneratePositionPublicId(string name) => _faker.Random.AlphaNumeric(15);

  /// <summary>
  /// Two-participant Standard guest draft with the fixed layout applied, positions
  /// assigned (owner -> "A" [7,6,4,2], other -> "B" [5,3,1]), and started. Create no
  /// longer auto-adds the owner, so both are added explicitly, exactly the same way.
  /// </summary>
  public static (
    Draft GuestDraft,
    DraftParticipant Owner,
    DraftParticipant Other
  ) CreateInProgressStandardGuestDraft()
  {
    var guestDraft = CreateGuestDraft(DraftType.Standard);
    var owner = AddParticipant(guestDraft, isOwner: true);
    var other = AddParticipant(guestDraft);

    guestDraft.UseFixedBoardLayout(GeneratePositionPublicId);

    var positions = guestDraft.GameBoard!.Positions.ToList();
    var positionA = positions.Single(p => p.Name == "A");
    var positionB = positions.Single(p => p.Name == "B");

    guestDraft.AssignParticipantToPosition(positionA, owner.Id.Value);
    guestDraft.AssignParticipantToPosition(positionB, other.Id.Value);

    guestDraft.Start();

    return (guestDraft, owner, other);
  }

  /// <summary>
  /// Custom-layout guest draft (MiniMega by default) with one position per
  /// participant, each carrying a single unique pick slot, fully assigned and
  /// started. Used for scenarios that need more than two participants. The first
  /// participant in the returned list is always the owner, added explicitly like
  /// everyone else.
  /// </summary>
  public static (
    Draft GuestDraft,
    IReadOnlyList<DraftParticipant> Participants,
    IReadOnlyList<DraftPosition> Positions
  ) CreateInProgressCustomGuestDraft(int participantCount, DraftType? guestDraftType = null)
  {
    var guestDraft = CreateGuestDraft(guestDraftType ?? DraftType.MiniMega);
    var participants = new List<DraftParticipant> { AddParticipant(guestDraft, isOwner: true) };

    for (var i = 1; i < participantCount; i++)
    {
      participants.Add(AddParticipant(guestDraft));
    }

    var customPositions = participants
      .Select(
        (_, index) =>
          (
            Name: $"Position {index + 1}",
            Picks: (IReadOnlyList<int>)new List<int> { index + 1 },
            HasBonusVeto: false,
            HasBonusVetoOverride: false,
            HasBonusFungibleToken: false
          )
      )
      .ToList();

    guestDraft.SetCustomPositions(customPositions, GeneratePositionPublicId);

    var positions = guestDraft.GameBoard!.Positions.ToList();

    for (var i = 0; i < participants.Count; i++)
    {
      guestDraft.AssignParticipantToPosition(positions[i], participants[i].Id.Value);
    }

    guestDraft.Start();

    return (guestDraft, participants, positions);
  }
}
