using ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts.Commands;

public sealed class CreateDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  public static readonly TheoryData<string, string, int, int, int, int, string> ValidRequests = new()
  {
    { Faker.Company.CompanyName(), DraftType.Standard.Name, 7, 2, 0, 2, DraftStatus.Created.Name  },
    { Faker.Company.CompanyName(), DraftType.Super.Name, 18, 4, 0, 2, DraftStatus.Created.Name  },
    { Faker.Company.CompanyName(), DraftType.Mega.Name, 20, 4, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.MiniMega.Name, 11, 3, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.MiniSuper.Name, 5, 2, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.Mega.Name, 25, 6, 0, 2, DraftStatus.Created.Name },
  };

  public static readonly TheoryData<string, string, int, int, int, int, string> InvalidRequests = new()
  {
    { string.Empty, DraftType.Standard.Name, 7, 2, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.Mega.Name, 0, 4, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.MiniMega.Name, 11, 0, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.MiniSuper.Name, 5, 2, 0, 0, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.Standard.Name, 7, 2, 0, 2, DraftStatus.Completed.Name },
  };

  public static readonly TheoryData<string, string, int, int, int, int, string> EmptyEnumRequests = new()
  {
    { Faker.Company.CompanyName(), string.Empty, 18, 4, 0, 2, DraftStatus.Created.Name  },
    { Faker.Company.CompanyName(), DraftType.Mega.Name, 25, 6, 0, 2, DraftStatus.Created.Name },
    { Faker.Company.CompanyName(), DraftType.Standard.Name, 7, 2, 0, 2, string.Empty },
  };

  [Theory]
  [MemberData(nameof(ValidRequests))]
  public async Task Should_ReturnOk_WhenRequestIsValidAsync(
    string title,
    string draftType,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeames,
    int totalHosts,
    string draftStatus)
  {
    // Arrange
    var request = new CreateDraftRequest(
    title,
     draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeames,
      totalHosts,
      draftStatus);

    // Act
    var command = new CreateDraftCommand(
      request.Title,
      DraftType.FromName(request.DraftType),
      request.TotalPicks,
      request.TotalDrafters,
      request.TotalDrafterTeams,
      request.TotalHosts,
      DraftStatus.FromName(request.DraftStatus));

    var draftId = await Sender.Send(command);

    // Assert
    draftId.Value.Should().NotBeEmpty();
    draftId.IsSuccess.Should().BeTrue();
  }

  [Theory]
  [MemberData(nameof(InvalidRequests))]
  public async Task Should_ReturnError_WhenRequestIsInvalidAsync(
    string title,
    string draftType,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts,
    string draftStatus)
  {
    // Arrange
    var request = new CreateDraftRequest(
    title,
     draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeams,
      totalHosts,
      draftStatus);

    // Act
    var command = new CreateDraftCommand(
      request.Title,
      DraftType.FromName(request.DraftType),
      request.TotalPicks,
      request.TotalDrafters,
      request.TotalDrafterTeams,
      request.TotalHosts,
      DraftStatus.FromName(request.DraftStatus));

    var draftId = await Sender.Send(command);

    // Assert
    draftId.Errors.Should().NotBeEmpty();
  }

  [Theory]
  [MemberData(nameof(EmptyEnumRequests))]
  public void Should_ThrowExceptions_WhenEnumsAreEmptyAsync(
    string title,
    string draftType,
    int totalPicks,
    int totalDrafters,
    int totalDrafterTeams,
    int totalHosts,
    string draftStatus)
  {
    // Arrange
    var request = new CreateDraftRequest(
    title,
     draftType,
      totalPicks,
      totalDrafters,
      totalDrafterTeams,
      totalHosts,
      draftStatus);

    // Act
    var exception = Assert.Throws<ArgumentException>(() =>
      new CreateDraftCommand(
        request.Title,
        DraftType.FromName(request.DraftType),
        request.TotalPicks,
        request.TotalDrafters,
        request.TotalDrafterTeams,
        request.TotalHosts,
        DraftStatus.FromName(request.DraftStatus)));


    // Assert
    Assert.Equal("Argument cannot be null or empty. (Parameter 'name')", exception.Message);
  }
}
