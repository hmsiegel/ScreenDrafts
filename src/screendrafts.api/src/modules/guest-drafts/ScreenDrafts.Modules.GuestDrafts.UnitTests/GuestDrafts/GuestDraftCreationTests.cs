namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftCreationTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var ownerUserId = Guid.NewGuid();
    var title = "Weekend Guest Draft";

    // Act
    var result = GuestDraft.Create(
      publicId,
      ownerUserId,
      title,
      GuestDraftType.Standard);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.OwnerUserId.Should().Be(ownerUserId);
    result.Value.Title.Should().Be(title);
    result.Value.GuestDraftType.Should().Be(GuestDraftType.Standard);
    result.Value.GuestDraftStatus.Should().Be(GuestDraftStatus.Created);
  }

  [Fact]
  public void Create_ShouldNotAddAnyParticipants()
  {
    // Arrange & Act -- the owner is no longer auto-added; they must be added
    // explicitly via AddParticipant, exactly like everyone else.
    var guestDraft = GuestDraft.Create(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      "Weekend Guest Draft",
      GuestDraftType.Standard).Value;

    // Assert
    guestDraft.Participants.Should().BeEmpty();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
  {
    // Arrange & Act
    var result = GuestDraft.Create(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      string.Empty,
      GuestDraftType.Standard);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.TitleIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsWhitespace()
  {
    // Arrange & Act
    var result = GuestDraft.Create(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      "   ",
      GuestDraftType.Standard);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.TitleIsRequired);
  }

  [Fact]
  public void AddParticipant_ShouldAddParticipant_WhenDraftIsCreated()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var guestDrafterId = Guid.NewGuid();
    var participant = GuestParticipant.From(GuestDrafterId.Create(guestDrafterId));

    // Act
    var result = guestDraft.AddParticipant(participant, isOwner: false);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.ParticipantIdValue.Should().Be(guestDrafterId);
    result.Value.IsOwner.Should().BeFalse();
    guestDraft.Participants.Should().HaveCount(1);
  }

  [Fact]
  public void AddParticipant_ShouldSetIsOwnerTrue_WhenCalledWithIsOwnerTrue()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var participant = GuestParticipant.From(GuestDrafterId.Create(Guid.NewGuid()));

    // Act
    var result = guestDraft.AddParticipant(participant, isOwner: true);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.IsOwner.Should().BeTrue();
  }

  [Fact]
  public void AddParticipant_ShouldReturnFailure_WhenTheSameParticipantIsAlreadyAdded()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var guestDrafterId = Guid.NewGuid();
    var participant = GuestParticipant.From(GuestDrafterId.Create(guestDrafterId));
    guestDraft.AddParticipant(participant, isOwner: false);

    // Act
    var result = guestDraft.AddParticipant(participant, isOwner: false);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.ParticipantAlreadyAdded(guestDrafterId));
    guestDraft.Participants.Should().HaveCount(1);
  }

  [Fact]
  public void AddParticipant_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var participant = GuestParticipant.From(GuestDrafterId.Create(Guid.NewGuid()));

    // Act
    var result = guestDraft.AddParticipant(participant, isOwner: false);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CannotAddParticipantAfterStart);
  }

  [Fact]
  public void HasParticipant_ShouldReturnTrue_WhenParticipantExists()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var owner = AddParticipant(guestDraft, isOwner: true);

    // Act & Assert
    guestDraft.HasParticipant(owner.Id.Value).Should().BeTrue();
  }

  [Fact]
  public void HasParticipant_ShouldReturnFalse_WhenParticipantDoesNotExist()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();

    // Act & Assert
    guestDraft.HasParticipant(Guid.NewGuid()).Should().BeFalse();
  }

  [Fact]
  public void FindParticipant_ShouldReturnNull_WhenParticipantDoesNotExist()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();

    // Act & Assert
    guestDraft.FindParticipant(Guid.NewGuid()).Should().BeNull();
  }

  [Fact]
  public void FindByParticipantRef_ShouldReturnTheParticipant_WhenItExists()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var guestDrafterId = Guid.NewGuid();
    var participantRef = GuestParticipant.From(GuestDrafterId.Create(guestDrafterId));
    var added = guestDraft.AddParticipant(participantRef, isOwner: false).Value;

    // Act
    var found = guestDraft.FindByParticipantRef(participantRef);

    // Assert
    found.Should().Be(added);
  }

  [Fact]
  public void FindByParticipantRef_ShouldReturnNull_WhenItDoesNotExist()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var participantRef = GuestParticipant.From(GuestDrafterId.Create(Guid.NewGuid()));

    // Act & Assert
    guestDraft.FindByParticipantRef(participantRef).Should().BeNull();
  }

  // ── SetDraftDate ─────────────────────────────────────────────────────────

  [Fact]
  public void SetDraftDate_ShouldSucceed_BeforeTheDraftHasStarted()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var draftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    // Act
    var result = guestDraft.SetDraftDate(draftDate);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.DraftDate.Should().Be(draftDate);
  }

  [Fact]
  public void SetDraftDate_ShouldSucceed_AfterTheDraftHasStarted()
  {
    // Arrange -- confirmed no status lock at all
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();
    var draftDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

    // Act
    var result = guestDraft.SetDraftDate(draftDate);

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.DraftDate.Should().Be(draftDate);
  }

  // ── SetTitle ─────────────────────────────────────────────────────────────

  [Fact]
  public void SetTitle_ShouldReturnFailure_WhenTitleIsEmpty()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();

    // Act
    var result = guestDraft.SetTitle(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.TitleIsRequired);
  }

  [Fact]
  public void SetTitle_ShouldReturnFailure_WhenTitleIsWhitespace()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();

    // Act
    var result = guestDraft.SetTitle("   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.TitleIsRequired);
  }

  [Fact]
  public void SetTitle_ShouldSucceed_BeforeTheDraftHasStarted()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();

    // Act
    var result = guestDraft.SetTitle("New Title");

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Title.Should().Be("New Title");
  }

  [Fact]
  public void SetTitle_ShouldSucceed_AfterTheDraftHasStarted()
  {
    // Arrange -- confirmed no status lock, matching SetDraftDate's pattern
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();

    // Act
    var result = guestDraft.SetTitle("New Title");

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDraft.Title.Should().Be("New Title");
  }
}
