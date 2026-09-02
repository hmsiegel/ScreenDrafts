namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

public class GuestDraftPositionTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();
    var publicId = Faker.Random.AlphaNumeric(15);
    var name = "A";
    var picks = new List<int> { 7, 6, 4, 2 };

    // Act
    var result = GuestDraftPosition.Create(gameBoardId, publicId, name, picks);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.GameBoardId.Should().Be(gameBoardId);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.Name.Should().Be(name);
    result.Value.Picks.Should().BeEquivalentTo(picks);
    result.Value.AssignedToParticipantId.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();

    // Act
    var result = GuestDraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), string.Empty, [1]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PositionNameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsWhitespace()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();

    // Act
    var result = GuestDraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "   ", [1]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PositionNameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPicksAreEmpty()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();

    // Act
    var result = GuestDraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "A", []);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PositionPicksAreRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsEmpty()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();

    // Act
    var result = GuestDraftPosition.Create(gameBoardId, string.Empty, "A", [1]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDraftErrors.PositionCreationFailed);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenPicksIsNull()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();
    IReadOnlyCollection<int>? picks = null;

    // Act
    Action act = () => GuestDraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "A", picks!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldSetBonusFlags_WhenProvided()
  {
    // Arrange
    var gameBoardId = GuestDraftGameBoardId.CreateUnique();

    // Act
    var result = GuestDraftPosition.Create(
      gameBoardId,
      Faker.Random.AlphaNumeric(15),
      "A",
      [1],
      hasBonusVeto: true,
      hasBonusVetoOverride: true,
      hasBonusFungibleToken: true);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HasBonusVeto.Should().BeTrue();
    result.Value.HasBonusVetoOverride.Should().BeTrue();
    result.Value.HasBonusFungibleToken.Should().BeTrue();
  }
}
