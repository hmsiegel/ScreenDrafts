namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafts;

public class GuestDraftCreationTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var ownerUserId = Guid.NewGuid();
    var ownerParticipantPublicId = Faker.Random.AlphaNumeric(10);
    var title = "Weekend Guest Draft";

    // Act
    var result = GuestDraft.Create(
      publicId,
      ownerUserId,
      ownerParticipantPublicId,
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
  public void Create_ShouldAddOwnerAsFirstParticipant_WithIsOwnerTrue()
  {
    // Arrange
    var ownerUserId = Guid.NewGuid();

    // Act
    var guestDraft = GuestDraft.Create(
      Faker.Random.AlphaNumeric(10),
      ownerUserId,
      Faker.Random.AlphaNumeric(10),
      "Weekend Guest Draft",
      GuestDraftType.Standard).Value;

    // Assert
    guestDraft.Participants.Should().HaveCount(1);
    var owner = guestDraft.Participants.Single();
    owner.UserId.Should().Be(ownerUserId);
    owner.IsOwner.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenTitleIsEmpty()
  {
    // Arrange & Act
    var result = GuestDraft.Create(
      Faker.Random.AlphaNumeric(10),
      Guid.NewGuid(),
      Faker.Random.AlphaNumeric(10),
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
      Faker.Random.AlphaNumeric(10),
      "   ",
      GuestDraftType.Standard);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.TitleIsRequired);
  }

  [Fact]
  public void InviteParticipant_ShouldAddParticipant_WhenDraftIsCreated()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var userId = Guid.NewGuid();

    // Act
    var result = guestDraft.InviteParticipant(Faker.Random.AlphaNumeric(10), userId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.UserId.Should().Be(userId);
    result.Value.IsOwner.Should().BeFalse();
    guestDraft.Participants.Should().HaveCount(2);
  }

  [Fact]
  public void InviteParticipant_ShouldReturnFailure_WhenUserIdIsAlreadyAParticipant()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var userId = Guid.NewGuid();
    guestDraft.InviteParticipant(Faker.Random.AlphaNumeric(10), userId);

    // Act
    var result = guestDraft.InviteParticipant(Faker.Random.AlphaNumeric(10), userId);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.ParticipantAlreadyAdded(userId));
    guestDraft.Participants.Should().HaveCount(2);
  }

  [Fact]
  public void InviteParticipant_ShouldReturnFailure_WhenStatusIsNotCreated()
  {
    // Arrange
    var (guestDraft, _, _) = CreateInProgressStandardGuestDraft();

    // Act
    var result = guestDraft.InviteParticipant(Faker.Random.AlphaNumeric(10), Guid.NewGuid());

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.CannotInviteAfterStart);
  }

  [Fact]
  public void HasParticipant_ShouldReturnTrue_WhenParticipantExists()
  {
    // Arrange
    var guestDraft = CreateGuestDraft();
    var owner = guestDraft.Participants.Single();

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
}
