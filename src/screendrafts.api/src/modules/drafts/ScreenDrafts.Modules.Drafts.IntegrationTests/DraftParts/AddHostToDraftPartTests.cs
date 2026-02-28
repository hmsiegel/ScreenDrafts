namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class AddHostToDraftPartTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddHostToDraftPart_WithValidPrimaryHost_ShouldSucceedAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task AddHostToDraftPart_ShouldPersistHostInDatabaseAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    };

    // Act
    await Sender.Send(command);

    // Assert
    var draftPart = await DbContext.DraftParts
      .Include("_draftHosts")
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));

    draftPart.DraftHosts.Should().NotBeEmpty();
    draftPart.PrimaryHost.Should().NotBeNull();
  }

  // -------------------------------------------------------------------------
  // Guard — draft part not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddHostToDraftPart_WithNonExistentDraftPart_ShouldFailAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = Guid.NewGuid(),
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — host not found
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddHostToDraftPart_WithNonExistentHost_ShouldFailAsync()
  {
    // Arrange
    var draftPartInternalId = await CreateDraftPartAsync();

    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = Faker.Random.AlphaNumeric(10),
      HostRole = HostRole.Primary
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Guard — primary host already set
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddHostToDraftPart_WhenPrimaryHostAlreadySet_ShouldFailAsync()
  {
    // Arrange — set a primary host, then try to set a different primary host
    var draftPartInternalId = await CreateDraftPartAsync();
    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var host1PublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = person1Id })).Value;

    // Set first primary host — should succeed
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = host1PublicId,
      HostRole = HostRole.Primary
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var host2PublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = person2Id })).Value;

    // Try to set a second (different) primary host — should fail
    var command = new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = host2PublicId,
      HostRole = HostRole.Primary
    };

    // Act
    var result = await Sender.Send(command);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<Guid> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
    return await GetFirstDraftPartIdAsync(draftPublicId);
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
