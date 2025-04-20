namespace ScreenDrafts.Modules.Drafts.UnitTests.Drafts.Entities;

public class HostTests : BaseTest
{
  [Fact]
  public void Create_ShouldReturnSuccessResult_WhenValidParametersAreProvided()
  {
    // Arrange
    var name = "Test Host";
    var userId = Guid.NewGuid();

    // Act
    var result = Host.Create(name, userId);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HostName.Should().Be(name);
    result.Value.UserId.Should().Be(userId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var name = string.Empty;
    var userId = Guid.NewGuid();

    // Act
    var result = Host.Create(name, userId);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  [Fact]
  public void UpdateName_ShouldUpdateName_WhenValidNameIsProvided()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var newName = Faker.Name.FullName();

    // Act
    host.UpdateHostName(newName);

    // Assert
    host.HostName.Should().Be(newName);
  }

  [Fact]
  public void UpdateName_ShouldReturnFailure_WhenNameIsEmpty()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var newName = string.Empty;

    // Act
    var result = host.UpdateHostName(newName);

    // Assert
    result.IsFailure.Should().BeTrue();
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
