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
    var hostName = Faker.Name.FullName();
    var host = Host.Create(hostName).Value;
    var draft = Draft.Create(
      Title. Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created).Value;

    // Act
    host.AddDraft(draft);

    // Assert
    Assert.Contains(draft, host.HostedDrafts);
  }

  [Fact]
  public void RemoveDraft_ValidDraft_RemovesDraftFromHostedDrafts()
  {
    // Arrange
    var hostName = Faker.Name.FullName();
    var host = Host.Create(hostName).Value;
    var draft = Draft.Create(
      Title. Create(Faker.Lorem.Word()),
      DraftType.Standard,
      7,
      2,
      2,
      DraftStatus.Created).Value;
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
    var hostName = Faker.Name.FullName();
    var host = Host.Create(hostName).Value;
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
    var hostName = Faker.Name.FullName();
    var host = Host.Create(hostName).Value;
    var firstName = Faker.Name.FirstName();
    var lastName = Faker.Name.LastName();

    // Act
    host.UpdateHostName(firstName, lastName);

    // Assert
    Assert.Equal($"{firstName} {lastName}", host.HostName);
  }
}
