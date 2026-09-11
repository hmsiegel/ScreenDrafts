using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;
using ScreenDrafts.Modules.GuestDrafts.Domain.DrafterTeams;

namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafterTeams;

public class GuestDrafterTeamTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var name = "The Contenders";
    var publicId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = DrafterTeam.Create(name, publicId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Name.Should().Be(name);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.NumberOfDrafters.Should().Be(0);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Act
    var result = DrafterTeam.Create(string.Empty, Faker.Random.AlphaNumeric(10));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidName);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsWhitespace()
  {
    // Act
    var result = DrafterTeam.Create("   ", Faker.Random.AlphaNumeric(10));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidName);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsEmpty()
  {
    // Act
    var result = DrafterTeam.Create("The Contenders", string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsWhitespace()
  {
    // Act
    var result = DrafterTeam.Create("The Contenders", "   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidPublicId);
  }

  [Fact]
  public void AddDrafter_ShouldSucceed_WhenDrafterIsNotAlreadyOnTheTeam()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;
    var drafter = CreateGuestDrafter();

    // Act
    var result = team.AddDrafter(drafter);

    // Assert
    result.IsSuccess.Should().BeTrue();
    team.Drafters.Should().ContainSingle().Which.Should().Be(drafter);
    team.NumberOfDrafters.Should().Be(1);
  }

  [Fact]
  public void AddDrafter_ShouldReturnFailure_WhenDrafterIsADuplicate()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;
    var drafter = CreateGuestDrafter();
    team.AddDrafter(drafter);

    // Act
    var result = team.AddDrafter(drafter);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.AlreadyAdded(drafter.Id.Value));
    team.NumberOfDrafters.Should().Be(1);
  }

  [Fact]
  public void RemoveDrafter_ShouldReturnFailure_WhenItWouldLeaveTheTeamWithZeroDrafters()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;
    var drafter = CreateGuestDrafter();
    team.AddDrafter(drafter);

    // Act
    var result = team.RemoveDrafter(drafter);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.NotEnoughDrafters);
    team.NumberOfDrafters.Should().Be(1);
  }

  [Fact]
  public void RemoveDrafter_ShouldSucceed_WhenAtLeastOneDrafterWouldRemain()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;
    var first = CreateGuestDrafter();
    var second = CreateGuestDrafter();
    team.AddDrafter(first);
    team.AddDrafter(second);

    // Act
    var result = team.RemoveDrafter(first);

    // Assert
    result.IsSuccess.Should().BeTrue();
    team.NumberOfDrafters.Should().Be(1);
    team.Drafters.Should().ContainSingle().Which.Should().Be(second);
  }

  [Fact]
  public void RemoveDrafter_ShouldReturnFailure_WhenDrafterIsNotOnTheTeam()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;
    team.AddDrafter(CreateGuestDrafter());
    var stranger = CreateGuestDrafter();

    // Act
    var result = team.RemoveDrafter(stranger);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterErrors.NotFound(stranger.Id.Value));
  }

  [Fact]
  public void UpdateName_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;

    // Act
    var result = team.UpdateName(string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidName);
    team.Name.Should().Be("The Contenders");
  }

  [Fact]
  public void UpdateName_ShouldReturnFailure_WhenNameIsWhitespace()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;

    // Act
    var result = team.UpdateName("   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DrafterTeamErrors.InvalidName);
  }

  [Fact]
  public void UpdateName_ShouldSucceed_WhenNameIsValid()
  {
    // Arrange
    var team = DrafterTeam.Create("The Contenders", Faker.Random.AlphaNumeric(10)).Value;

    // Act
    var result = team.UpdateName("The Champions");

    // Assert
    result.IsSuccess.Should().BeTrue();
    team.Name.Should().Be("The Champions");
  }

  private static Drafter CreateGuestDrafter() =>
    Drafter
      .Create(
        Faker.Random.AlphaNumeric(10),
        Guid.NewGuid(),
        Faker.Name.FirstName(),
        Faker.Name.LastName()
      )
      .Value;
}
