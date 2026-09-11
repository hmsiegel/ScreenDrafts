using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.Errors;
using ScreenDrafts.Modules.GuestDrafts.Domain.Drafts.ValueObjects;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.Entities;

public class GuestDraftPositionTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();
    var publicId = Faker.Random.AlphaNumeric(15);
    var name = "A";
    var picks = new List<int> { 7, 6, 4, 2 };

    // Act
    var result = DraftPosition.Create(gameBoardId, publicId, name, picks);

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
    var gameBoardId = GameBoardId.CreateUnique();

    // Act
    var result = DraftPosition.Create(
      gameBoardId,
      Faker.Random.AlphaNumeric(15),
      string.Empty,
      [1]
    );

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionNameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsWhitespace()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();

    // Act
    var result = DraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "   ", [1]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionNameIsRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPicksAreEmpty()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();

    // Act
    var result = DraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "A", []);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionPicksAreRequired);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsEmpty()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();

    // Act
    var result = DraftPosition.Create(gameBoardId, string.Empty, "A", [1]);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftErrors.PositionCreationFailed);
  }

  [Fact]
  public void Create_ShouldThrowArgumentNullException_WhenPicksIsNull()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();
    IReadOnlyCollection<int>? picks = null;

    // Act
    Action act = () =>
      DraftPosition.Create(gameBoardId, Faker.Random.AlphaNumeric(15), "A", picks!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Create_ShouldSetBonusFlags_WhenProvided()
  {
    // Arrange
    var gameBoardId = GameBoardId.CreateUnique();

    // Act
    var result = DraftPosition.Create(
      gameBoardId,
      Faker.Random.AlphaNumeric(15),
      "A",
      [1],
      hasBonusVeto: true,
      hasBonusVetoOverride: true,
      hasBonusFungibleToken: true
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HasBonusVeto.Should().BeTrue();
    result.Value.HasBonusVetoOverride.Should().BeTrue();
    result.Value.HasBonusFungibleToken.Should().BeTrue();
  }
}
