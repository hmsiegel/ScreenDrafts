namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class HostTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var person = PersonFactory.CreatePerson().Value;

    // Act
    var result = Host.Create(person);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Person.Should().Be(person);
    result.Value.Person.FirstName.Should().Be(person.FirstName);
  }
}
