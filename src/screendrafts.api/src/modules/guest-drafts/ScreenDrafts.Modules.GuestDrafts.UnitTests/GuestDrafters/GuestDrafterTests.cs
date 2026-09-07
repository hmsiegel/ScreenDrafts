namespace ScreenDrafts.Modules.GuestDrafts.UnitTests.GuestDrafters;

public class GuestDrafterTests : GuestDraftsBaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var publicId = Faker.Random.AlphaNumeric(10);
    var userId = Guid.NewGuid();
    var firstName = "Jane";
    var lastName = "Doe";

    // Act
    var result = GuestDrafter.Create(publicId, userId, firstName, lastName);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.UserId.Should().Be(userId);
    result.Value.FirstName.Should().Be(firstName);
    result.Value.LastName.Should().Be(lastName);
    result.Value.DisplayName.Should().Be("Jane Doe");
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenFirstNameIsEmpty()
  {
    // Act
    var result = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), string.Empty, "Doe");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidFirstName);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenFirstNameIsWhitespace()
  {
    // Act
    var result = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "   ", "Doe");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidFirstName);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenLastNameIsEmpty()
  {
    // Act
    var result = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "Jane", string.Empty);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidLastName);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenLastNameIsWhitespace()
  {
    // Act
    var result = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "Jane", "   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidLastName);
  }

  [Fact]
  public void UpdateName_ShouldReturnFailure_WhenFirstNameIsEmpty()
  {
    // Arrange
    var guestDrafter = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "Jane", "Doe").Value;

    // Act
    var result = guestDrafter.UpdateName(string.Empty, "Smith");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidFirstName);
    guestDrafter.FirstName.Should().Be("Jane");
  }

  [Fact]
  public void UpdateName_ShouldReturnFailure_WhenLastNameIsWhitespace()
  {
    // Arrange
    var guestDrafter = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "Jane", "Doe").Value;

    // Act
    var result = guestDrafter.UpdateName("Janet", "   ");

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(GuestDrafterErrors.InvalidLastName);
    guestDrafter.LastName.Should().Be("Doe");
  }

  [Fact]
  public void UpdateName_ShouldUpdateFirstAndLastNameAndDisplayName_WhenValid()
  {
    // Arrange
    var guestDrafter = GuestDrafter.Create(Faker.Random.AlphaNumeric(10), Guid.NewGuid(), "Jane", "Doe").Value;

    // Act
    var result = guestDrafter.UpdateName("Janet", "Smith");

    // Assert
    result.IsSuccess.Should().BeTrue();
    guestDrafter.FirstName.Should().Be("Janet");
    guestDrafter.LastName.Should().Be("Smith");
    guestDrafter.DisplayName.Should().Be("Janet Smith");
  }
}
