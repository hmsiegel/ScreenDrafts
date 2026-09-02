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

  public static GuestDraft CreateGuestDraft(
    GuestDraftType? guestDraftType = null,
    Guid? ownerUserId = null,
    string? title = null)
  {
    return GuestDraft.Create(
      publicId: _faker.Random.AlphaNumeric(10),
      ownerUserId: ownerUserId ?? Guid.NewGuid(),
      ownerParticipantPublicId: _faker.Random.AlphaNumeric(10),
      title: title ?? _faker.Lorem.Sentence(3),
      guestDraftType: guestDraftType ?? GuestDraftType.Standard).Value;
  }

  public static GuestDraftParticipant InviteParticipant(GuestDraft guestDraft, Guid? userId = null)
  {
    ArgumentNullException.ThrowIfNull(guestDraft);

    return guestDraft
      .InviteParticipant(_faker.Random.AlphaNumeric(10), userId ?? Guid.NewGuid())
      .Value;
  }

  public static string GeneratePositionPublicId(string name) => _faker.Random.AlphaNumeric(15);

  /// <summary>
  /// Two-participant Standard guest draft with the fixed layout applied, positions
  /// assigned (owner -> "A" [7,6,4,2], other -> "B" [5,3,1]), and started.
  /// </summary>
  public static (GuestDraft GuestDraft, GuestDraftParticipant Owner, GuestDraftParticipant Other)
    CreateInProgressStandardGuestDraft()
  {
    var guestDraft = CreateGuestDraft(GuestDraftType.Standard);
    var owner = guestDraft.Participants.Single();
    var other = InviteParticipant(guestDraft);

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
  /// started. Used for scenarios that need more than two participants.
  /// </summary>
  public static (GuestDraft GuestDraft, IReadOnlyList<GuestDraftParticipant> Participants, IReadOnlyList<GuestDraftPosition> Positions)
    CreateInProgressCustomGuestDraft(int participantCount, GuestDraftType? guestDraftType = null)
  {
    var guestDraft = CreateGuestDraft(guestDraftType ?? GuestDraftType.MiniMega);
    var participants = new List<GuestDraftParticipant> { guestDraft.Participants.Single() };

    for (var i = 1; i < participantCount; i++)
    {
      participants.Add(InviteParticipant(guestDraft));
    }

    var customPositions = participants
      .Select((_, index) => (
        Name: $"Position {index + 1}",
        Picks: (IReadOnlyList<int>)new List<int> { index + 1 },
        HasBonusVeto: false,
        HasBonusVetoOverride: false,
        HasBonusFungibleToken: false))
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
