using ScreenDrafts.Modules.Drafts.Features.People.Create;
using ScreenDrafts.Modules.Drafts.Features.People.Get;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class GetPersonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetPerson_WithValidId_ShouldReturnPersonAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var person = peopleFactory.CreatePerson();
    var createCommand = new CreatePersonCommand
    {
      FirstName = person.FirstName,
      LastName = person.LastName
    };
    var personId = await Sender.Send(createCommand);
    // Act
    var result = await Sender.Send(new GetPersonQuery(personId.Value));
    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(personId.Value);
    result.Value.FirstName.Should().Be(person.FirstName);
    result.Value.LastName.Should().Be(person.LastName);
  }

  [Fact]
  public async Task GetPerson_WithInvalidId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidId = string.Empty;
    // Act
    var result = await Sender.Send(new GetPersonQuery(invalidId));
    // Assert
    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(PersonErrors.NotFound(invalidId));
  }
}
