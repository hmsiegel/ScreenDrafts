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

  [Fact]
  public void AddDraft_ShouldAddDraftToList_WhenValidDraftIsProvided()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var draft = DraftFactory.CreateStandardDraft().Value;

    // Act
    host.AddDraft(draft);

    // Assert
    host.HostedDrafts.Should().ContainSingle();
    host.HostedDrafts.First().Should().Be(draft);
  }

  [Fact]
  public void RemoveDraft_ShouldRemoveDraftFromList_WhenDraftExists()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var draft = DraftFactory.CreateStandardDraft().Value;
    host.AddDraft(draft);

    // Act
    host.RemoveDraft(draft);

    // Assert
    host.HostedDrafts.Should().BeEmpty();
  }

  [Fact]
  public void RemoveDraft_ShouldReturnFailure_WhenDraftDoesNotExist()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var nonExistentDraft = DraftFactory.CreateStandardDraft().Value;

    // Act
    var result = host.RemoveDraft(nonExistentDraft);

    // Assert
    result.IsFailure.Should().BeTrue();
  }
}
