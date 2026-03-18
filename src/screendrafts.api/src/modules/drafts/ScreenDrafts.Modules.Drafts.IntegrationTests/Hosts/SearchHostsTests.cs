namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Hosts;

public sealed class SearchHostsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task SearchHosts_WithNoHosts_ShouldReturnEmptyAsync()
  {
    // Act
    var result = await Sender.Send(new SearchHostQuery());

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchHosts_WithNoFilter_ShouldReturnAllHostsAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    var id1 = await hostFactory.CreateAndSaveHostAsync();
    var id2 = await hostFactory.CreateAndSaveHostAsync();
    var id3 = await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { PageSize = 10 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(3);
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items.Should().Contain(h => h.PublicId == id1);
    result.Value.Items.Should().Contain(h => h.PublicId == id2);
    result.Value.Items.Should().Contain(h => h.PublicId == id3);
  }

  [Fact]
  public async Task SearchHosts_ByName_ShouldReturnMatchingHostsAsync()
  {
    // Arrange
    var uniqueSuffix = "UniqueHost_" + Faker.Random.AlphaNumeric(6);
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostWithNameAsync("Alpha" + uniqueSuffix, "One");
    await hostFactory.CreateAndSaveHostWithNameAsync("Beta" + uniqueSuffix, "Two");
    await hostFactory.CreateAndSaveHostAsync(); // unrelated host

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = uniqueSuffix });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(2);
    result.Value.Items.Should().HaveCount(2);
  }

  [Fact]
  public async Task SearchHosts_ByName_ShouldBeCaseInsensitiveAsync()
  {
    // Arrange
    var lastName = "Casesensitive_" + Faker.Random.AlphaNumeric(6);
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostWithNameAsync("First", lastName);

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = lastName.ToUpperInvariant() });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items[0].LastName.Should().Be(lastName);
  }

  [Fact]
  public async Task SearchHosts_ByName_ShouldMatchPartialNameAsync()
  {
    // Arrange
    var suffix = "Partial_" + Faker.Random.AlphaNumeric(6);
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostWithNameAsync("John", "Prefix_" + suffix);

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = suffix });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.TotalCount.Should().Be(1);
    result.Value.Items[0].LastName.Should().Contain(suffix);
  }

  [Fact]
  public async Task SearchHosts_ByName_WithNoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = "zzz_no_match_" + Faker.Random.AlphaNumeric(12) });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchHosts_WithPagination_ShouldReturnFirstPageAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostAsync();
    await hostFactory.CreateAndSaveHostAsync();
    await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { Page = 1, PageSize = 2 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Page.Should().Be(1);
    result.Value.TotalPages.Should().Be(2);
    result.Value.HasNextPage.Should().BeTrue();
    result.Value.HasPreviousPage.Should().BeFalse();
  }

  [Fact]
  public async Task SearchHosts_SecondPage_ShouldReturnRemainingItemsAsync()
  {
    // Arrange
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostAsync();
    await hostFactory.CreateAndSaveHostAsync();
    await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { Page = 2, PageSize = 2 });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.TotalCount.Should().Be(3);
    result.Value.Page.Should().Be(2);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeFalse();
  }

  [Fact]
  public async Task SearchHosts_ResponseShape_ShouldIncludeAllFieldsAsync()
  {
    // Arrange
    var firstName = "John";
    var lastName = "Shape_" + Faker.Random.AlphaNumeric(6);
    var hostFactory = new HostFactory(Sender, Faker);
    var (hostId, _, _) = await hostFactory.CreateAndSaveHostWithNameAsync(firstName, lastName);

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = lastName });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    var item = result.Value.Items[0];
    item.PublicId.Should().Be(hostId);
    item.FirstName.Should().Be(firstName);
    item.LastName.Should().Be(lastName);
    item.PersonPublicId.Should().NotBeNullOrEmpty();
    item.HostedDraftPartCount.Should().Be(0);
  }

  [Fact]
  public async Task SearchHosts_ShouldReturnResultsOrderedByLastNameThenFirstNameAsync()
  {
    // Arrange
    var suffix = "Order_" + Faker.Random.AlphaNumeric(6);
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostWithNameAsync("Charlie", suffix + "_C");
    await hostFactory.CreateAndSaveHostWithNameAsync("Alice", suffix + "_A");
    await hostFactory.CreateAndSaveHostWithNameAsync("Bob", suffix + "_B");

    // Act
    var result = await Sender.Send(new SearchHostQuery { Name = suffix });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items[0].LastName.Should().EndWith("_A");
    result.Value.Items[1].LastName.Should().EndWith("_B");
    result.Value.Items[2].LastName.Should().EndWith("_C");
  }

  [Fact]
  public async Task SearchHosts_FilterByHasBeenPrimary_ShouldOnlyReturnPrimaryHostsAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);

    // Create a host and add them as a primary host to a draft part
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var primaryHostId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var draftPartPublicId = await CreateDraftPartAsync();
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = primaryHostId,
      HostRole = HostRole.Primary
    });

    // Create a host that has never been primary
    var hostFactory = new HostFactory(Sender, Faker);
    await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { HasBeenPrimary = true });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].PublicId.Should().Be(primaryHostId);
  }

  [Fact]
  public async Task SearchHosts_FilterByHasNotBeenPrimary_ShouldExcludePrimaryHostsAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);

    // Create a host and add them as a primary host to a draft part
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var primaryHostId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    var draftPartPublicId = await CreateDraftPartAsync();
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = primaryHostId,
      HostRole = HostRole.Primary
    });

    // Create a host that has never been primary
    var hostFactory = new HostFactory(Sender, Faker);
    var nonPrimaryHostId = await hostFactory.CreateAndSaveHostAsync();

    // Act
    var result = await Sender.Send(new SearchHostQuery { HasBeenPrimary = false });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items[0].PublicId.Should().Be(nonPrimaryHostId);
  }

  [Fact]
  public async Task SearchHosts_HostedDraftPartCount_ShouldReflectActualHostingCountAsync()
  {
    // Arrange
    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    // Add host to two draft parts
    var draftPartPublicId1 = await CreateDraftPartAsync();
    var draftPartPublicId2 = await CreateDraftPartAsync();

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId1,
      HostPublicId = hostId,
      HostRole = HostRole.Primary
    });
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId2,
      HostPublicId = hostId,
      HostRole = HostRole.Primary
    });

    // Act - query by person's host ID
    var result = await Sender.Send(new SearchHostQuery { HasBeenPrimary = true });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.SingleOrDefault(h => h.PublicId == hostId);
    item.Should().NotBeNull();
    item!.HostedDraftPartCount.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

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
