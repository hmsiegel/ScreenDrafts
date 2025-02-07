namespace ScreenDrafts.Modules.Drafts.UnitTests.Hosts;

public class HostTests : BaseTest
{
  [Fact]
  public void Create_ValidHostName_ReturnsSuccessResult()
  {
    // Arrange
    var hostName = Faker.Name.FullName();
    var userId = Guid.NewGuid();

    // Act
    var result = Host.Create(hostName, userId);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(hostName, result.Value.HostName);
    Assert.Equal(userId, result.Value.UserId);
  }

  [Fact]
  public void Create_InvalidHostName_ReturnsFailureResult()
  {
    // Arrange
    var hostName = "";

    // Act
    var result = Host.Create(hostName);

    // Assert
    Assert.False(result.IsSuccess);
    Assert.Equal(HostErrors.InvalidHostName, result.Errors[0]);
  }

  [Fact]
  public void AddDraft_ValidDraft_AddsDraftToHostedDrafts()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var draft = DraftFactory.CreateStandardDraft();

    // Act
    host.AddDraft(draft.Value);

    // Assert
    Assert.Contains(draft.Value, host.HostedDrafts);
  }

  [Fact]
  public void RemoveDraft_ValidDraft_RemovesDraftFromHostedDrafts()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var draft = DraftFactory.CreateStandardDraft().Value;
    host.AddDraft(draft);

    // Act
    host.RemoveDraft(draft);

    // Assert
    Assert.DoesNotContain(draft, host.HostedDrafts);
  }

  [Fact]
  public void UpdateHostName_ValidNames_UpdatesHostName()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();
    var middleName = Faker.Name.FirstName();

    // Act
    host.UpdateHostName(firstName, lastName, middleName);

    // Assert
    Assert.Equal($"{firstName} {middleName} {lastName}", host.HostName);
  }

  [Fact]
  public void UpdateHostName_WithoutMiddleName_UpdatesHostName()
  {
    // Arrange
    var host = HostsFactory.CreateHost().Value;
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();

    // Act
    host.UpdateHostName(firstName, lastName);

    // Assert
    Assert.Equal($"{firstName} {lastName}", host.HostName);
  }
}
