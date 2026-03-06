namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class GetHostTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task GetHost_WithValidId_ShouldReturnHostAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    var hostId = await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = hostId });

    // Assert
    result.Should().NotBeNull();
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(hostId);
  }

  [Fact]
  public async Task GetHost_WithInvalidId_ShouldReturnErrorAsync()
  {
    // Arrange
    var nonExistentId = Faker.Random.AlphaNumeric(10);

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = nonExistentId });

    // Assert
    result.Should().NotBeNull();
    result.IsFailure.Should().BeTrue();
    result.Errors.Should().NotBeEmpty();
    result.Errors[0].Should().Be(HostErrors.NotFound(nonExistentId));
  }

  [Fact]
  public async Task GetHost_ResponseShape_ShouldIncludePersonDetailsAsync()
  {
    // Arrange
    var firstName = "Jane";
    var lastName = "Smith";
    var hostFactory = new HostFactory(Sender, Faker);
    var (hostId, _, _) = await hostFactory.CreateAndSaveHostWithNameAsync(firstName, lastName);

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = hostId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.FirstName.Should().Be(firstName);
    result.Value.LastName.Should().Be(lastName);
    result.Value.PublicPersonId.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetHost_WithNoHostedDraftParts_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    var hostId = await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = hostId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HostedDraftParts.Should().BeEmpty();
  }

  [Fact]
  public async Task GetHost_WithHostedDraftPart_ShouldReturnDraftPartDetailsAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var draftPartInternalId = await CreateDraftPartAsync();
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = hostId,
      HostRole = HostRole.Primary
    });

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = hostId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HostedDraftParts.Should().HaveCount(1);
    var hostedPart = result.Value.HostedDraftParts[0];
    hostedPart.DraftPartPublicId.Should().NotBeNullOrEmpty();
    hostedPart.DraftPublicId.Should().NotBeNullOrEmpty();
    hostedPart.Label.Should().NotBeNullOrEmpty();
    hostedPart.Role.Should().NotBeNullOrEmpty();
    hostedPart.Status.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task GetHost_HostedDraftPart_LabelShouldBeDraftTitleForPartIndex1Async()
  {
    // Arrange
    var draftTitle = "My Draft " + Faker.Random.AlphaNumeric(6);
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftWithTitleAsync(seriesId, draftTitle);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = hostId,
      HostRole = HostRole.Primary
    });

    // Act
    var result = await Sender.Send(new GetHostQuery { HostPublicId = hostId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.HostedDraftParts.Should().HaveCount(1);
    result.Value.HostedDraftParts[0].Label.Should().Be(draftTitle);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<Guid> CreateDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftWithTitleAsync(seriesId, Faker.Company.CompanyName());
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

  private async Task<string> CreateDraftWithTitleAsync(Guid seriesId, string title)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = title,
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
