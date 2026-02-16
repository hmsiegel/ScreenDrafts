using ScreenDrafts.Modules.Drafts.Domain.People;
using ScreenDrafts.Modules.Drafts.Features.People.Get;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class GetPersonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetPerson_WithValidPublicId_ShouldReturnPersonAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();
    var publicId = await peopleFactory.CreateAndSavePersonWithNameAsync(firstName, lastName);

    // Act
    var result = await Sender.Send(new GetPersonQuery(publicId));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
    result.Value.FirstName.Should().Be(firstName);
    result.Value.LastName.Should().Be(lastName);
  }

  [Fact]
  public async Task GetPerson_WithInvalidPublicId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidPublicId = "person_" + Guid.NewGuid().ToString();

    // Act
    var result = await Sender.Send(new GetPersonQuery(invalidPublicId));

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.NotFound(invalidPublicId));
  }

  [Fact]
  public async Task GetPerson_WithEmptyPublicId_ShouldReturnErrorAsync()
  {
    // Arrange
    var emptyPublicId = string.Empty;

    // Act
    var result = await Sender.Send(new GetPersonQuery(emptyPublicId));

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task GetPerson_WithUserId_ShouldReturnPersonWithUserIdAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    // Act
    var result = await Sender.Send(new GetPersonQuery(publicId));

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(publicId);
  }
}
