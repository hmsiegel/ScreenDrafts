namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.Invitations;

/// <summary>Builders shared by the Invitations handler tests (no database involved).</summary>
internal static class InvitationTestData
{
  public static readonly DateTime Now = new(2026, 10, 11, 12, 0, 0, DateTimeKind.Utc);

  private static string Suffix() => TestFakerProvider.Faker.Random.AlphaNumeric(15);

  public static string NewUserPublicId() => $"u_{Suffix()}";

  public static Draft CreateDraft(Guid? ownerUserId = null, string title = "Friday Night Draft") =>
    Draft
      .Create($"gd_{Suffix()}", ownerUserId ?? Guid.NewGuid(), title, DraftType.Standard)
      .Value;

  /// <summary>A Standard draft with two participants and a full board that has been started.</summary>
  public static Draft CreateStartedDraft(Guid? ownerUserId = null)
  {
    var draft = CreateDraft(ownerUserId);
    var owner = draft
      .AddParticipant(Participant.From(DrafterId.CreateUnique()), isOwner: true)
      .Value;
    var other = draft.AddParticipant(Participant.From(DrafterId.CreateUnique()), isOwner: false).Value;

    draft.UseFixedBoardLayout(_ => $"p_{Suffix()}").IsSuccess.Should().BeTrue();

    var positions = draft.GameBoard!.Positions.ToList();
    draft.AssignParticipantToPosition(positions.Single(p => p.Name == "A"), owner.Id.Value);
    draft.AssignParticipantToPosition(positions.Single(p => p.Name == "B"), other.Id.Value);
    draft.Start().IsSuccess.Should().BeTrue("test setup must be able to start the draft");

    return draft;
  }

  public static Invitation CreateInvitation(
    Draft draft,
    string email = "guest@example.com",
    string name = "Guest",
    DateTime? sentAt = null,
    Guid? invitedBy = null
  )
  {
    var invitation = Invitation
      .Create(
        publicId: $"gdi_{Suffix()}",
        draftId: draft.Id,
        draftPublicId: draft.PublicId,
        email: email,
        name: name,
        invitedByUserId: invitedBy ?? draft.OwnerUserId,
        utcNow: sentAt ?? Now
      )
      .Value;

    invitation.ClearDomainEvents();
    return invitation;
  }

  public static Drafter CreateDrafter(Guid userId, string first = "Gina", string last = "Guest") =>
    Drafter.Create($"gdr_{Suffix()}", userId, first, last).Value;
}
