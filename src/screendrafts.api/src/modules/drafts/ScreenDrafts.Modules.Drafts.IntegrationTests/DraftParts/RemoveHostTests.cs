namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class RemoveHostTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveHost_WhenHostExistsOnDraftPart_ShouldSucceedAsync()
  {
    // Arrange
    var (draftPartPublicId, hostPublicId) = await SetupDraftPartWithHostAsync();

    var command = new RemoveHostDraftPartCommand { DraftPartId = draftPartPublicId, HostId = hostPublicId };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task RemoveHost_ShouldRemoveHostFromDatabaseAsync()
  {
    // Arrange
    var (draftPartPublicId, hostPublicId) = await SetupDraftPartWithHostAsync();

    var command = new RemoveHostDraftPartCommand { DraftPartId = draftPartPublicId, HostId = hostPublicId };

    // Act
    await Sender.Send(command);

    // Assert
    var draftPart = await DbContext.DraftParts
      .Include("_draftHosts")
      .FirstAsync(dp => dp.PublicId == draftPartPublicId);

    draftPart.DraftHosts.Should().BeEmpty();
    draftPart.PrimaryHost.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveHost_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var command = new RemoveHostDraftPartCommand { DraftPartId = Faker.Random.AlphaNumeric(21), HostId = hostPublicId };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — host not on draft part
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveHost_WhenHostNotOnDraftPart_ShouldFailAsync()
  {
    // Arrange
    var draftPartPublicId = await CreateDraftPartAsync();

    // Create a host but do NOT add it to the draft part
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var command = new RemoveHostDraftPartCommand { DraftPartId = draftPartPublicId, HostId = hostPublicId };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string HostPublicId)> SetupDraftPartWithHostAsync()
  {
    var draftPartPublicId = await CreateDraftPartAsync();

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    return (draftPartPublicId, hostPublicId);
  }

  private async Task<string> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    return await GetFirstDraftPartPublicIdAsync(draftPublicId);
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    return result.Value;
  }

  private async Task<string> CreateDraftAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId,
    });

    var draftPublicId = draftResult.Value;
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    });

    return draftPublicId;
  }
}
