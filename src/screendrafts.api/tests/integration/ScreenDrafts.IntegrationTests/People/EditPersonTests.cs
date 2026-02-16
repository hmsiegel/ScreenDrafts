using ScreenDrafts.Modules.Drafts.Features.People.Edit;
using ScreenDrafts.Modules.Drafts.Features.People.Get;
using ScreenDrafts.Modules.Drafts.IntegrationTests.TestUtils;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.People;

public sealed class EditPersonTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task EditPerson_WithValidData_ShouldUpdatePersonAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var newFirstName = Faker.Name.FirstName();
    var newLastName = Faker.Name.LastName();
    var command = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = newFirstName,
      LastName = newLastName
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getResult = await Sender.Send(new GetPersonQuery(publicId));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.FirstName.Should().Be(newFirstName);
    getResult.Value.LastName.Should().Be(newLastName);
  }

  [Fact]
  public async Task EditPerson_WithDisplayName_ShouldUpdateDisplayNameAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var displayName = Faker.Name.FullName();
    var command = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName(),
      DisplayName = displayName
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsSuccess.Should().BeTrue();

    var getResult = await Sender.Send(new GetPersonQuery(publicId));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.DisplayName.Should().Be(displayName);
  }

  [Fact]
  public async Task EditPerson_WithInvalidPublicId_ShouldReturnErrorAsync()
  {
    // Arrange
    var invalidPublicId = "person_" + Guid.NewGuid().ToString();
    var command = new EditPersonCommand
    {
      PublicId = invalidPublicId,
      FirstName = Faker.Name.FirstName(),
      LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditPerson_WithEmptyFirstName_ShouldReturnErrorAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var command = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = string.Empty,
      LastName = Faker.Name.LastName()
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditPerson_WithEmptyLastName_ShouldReturnErrorAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    var command = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = Faker.Name.FirstName(),
      LastName = string.Empty
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public async Task EditPerson_MultipleEdits_ShouldUpdateSuccessfullyAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var publicId = await peopleFactory.CreateAndSavePersonAsync();

    // Act - First Edit
    var firstEdit = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = "John",
      LastName = "Doe"
    };
    var firstResult = await Sender.Send(firstEdit);

    // Act - Second Edit
    var secondEdit = new EditPersonCommand
    {
      PublicId = publicId,
      FirstName = "Jane",
      LastName = "Smith"
    };
    var secondResult = await Sender.Send(secondEdit);

    // Assert
    firstResult.IsSuccess.Should().BeTrue();
    secondResult.IsSuccess.Should().BeTrue();

    var getResult = await Sender.Send(new GetPersonQuery(publicId));
    getResult.IsSuccess.Should().BeTrue();
    getResult.Value.FirstName.Should().Be("Jane");
    getResult.Value.LastName.Should().Be("Smith");
  }
}
