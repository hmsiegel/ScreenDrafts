namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.Entities;

public class DraftHostTests : DraftsBaseTest
{
  [Fact]
  public void CreatePrimary_ShouldCreateDraftHostWithPrimaryRole()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var host = CreateHost();

    // Act
    var draftHost = DraftHost.CreatePrimary(draftPart, host);

    // Assert
    draftHost.Should().NotBeNull();
    draftHost.DraftPart.Should().Be(draftPart);
    draftHost.DraftPartId.Should().Be(draftPart.Id);
    draftHost.Host.Should().Be(host);
    draftHost.HostId.Should().Be(host.Id);
    draftHost.Role.Should().Be(HostRole.Primary);
  }

  [Fact]
  public void CreateCoHost_ShouldCreateDraftHostWithCoHostRole()
  {
    // Arrange
    var draftPart = CreateDraftPart();
    var host = CreateHost();

    // Act
    var draftHost = DraftHost.CreateCoHost(draftPart, host);

    // Assert
    draftHost.Should().NotBeNull();
    draftHost.DraftPart.Should().Be(draftPart);
    draftHost.DraftPartId.Should().Be(draftPart.Id);
    draftHost.Host.Should().Be(host);
    draftHost.HostId.Should().Be(host.Id);
    draftHost.Role.Should().Be(HostRole.CoHost);
  }

  [Fact]
  public void CreatePrimary_ShouldThrowArgumentNullException_WhenDraftPartIsNull()
  {
    // Arrange
    var host = CreateHost();

    // Act
    Action act = () => DraftHost.CreatePrimary(null!, host);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void CreatePrimary_ShouldThrowArgumentNullException_WhenHostIsNull()
  {
    // Arrange
    var draftPart = CreateDraftPart();

    // Act
    Action act = () => DraftHost.CreatePrimary(draftPart, null!);

    // Assert
    act.Should().Throw<ArgumentNullException>();
  }
}
