namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.CandidateLists;

public sealed class GetCandidateListTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetCandidateList_ShouldReturnEmptyList_WhenNoEntriesExistAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 50
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Response.Items.Should().BeEmpty();
    result.Value.Response.TotalCount.Should().Be(0);
  }

  [Fact]
  public async Task GetCandidateList_ShouldReturnEntries_WhenEntriesExistAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);
    var tmdbId1 = Faker.Random.Int(1, 500_000);
    var tmdbId2 = Faker.Random.Int(500_001, 1_000_000);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId1,
      AddedByPublicId = addedByPublicId
    });

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId2,
      AddedByPublicId = addedByPublicId
    });

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 50
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Response.TotalCount.Should().Be(2);
    result.Value.Response.Items.Should().HaveCount(2);
    result.Value.Response.Items.Should().Contain(i => i.TmdbId == tmdbId1);
    result.Value.Response.Items.Should().Contain(i => i.TmdbId == tmdbId2);
  }

  [Fact]
  public async Task GetCandidateList_ShouldSetAddedByPublicId_WhenEntryAddedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = addedByPublicId
    });

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 50
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    var item = result.Value.Response.Items.First(i => i.TmdbId == tmdbId);
    item.AddedByPublicId.Should().Be(addedByPublicId);
  }

  [Fact]
  public async Task GetCandidateList_ShouldIncludeMovieTitle_WhenMovieIsResolvedAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    await CreateMovieInDbAsync(tmdbId);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    });

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 50
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    var item = result.Value.Response.Items.First(i => i.TmdbId == tmdbId);
    item.MovieTitle.Should().Be("Test Movie");
    item.IsPending.Should().BeFalse();
  }

  [Fact]
  public async Task GetCandidateList_ShouldHaveNullMovieTitle_WhenEntryIsPendingAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    await Sender.Send(new AddCandidateEntryCommand
    {
      DraftPartId = draftPartPublicId,
      TmdbId = tmdbId,
      AddedByPublicId = "u_" + Faker.Random.AlphaNumeric(17)
    });

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 50
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    var item = result.Value.Response.Items.First(i => i.TmdbId == tmdbId);
    item.MovieTitle.Should().BeNull();
    item.IsPending.Should().BeTrue();
  }

  // -------------------------------------------------------------------------
  // Pagination
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetCandidateList_ShouldSupportPaginationAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);

    for (var i = 0; i < 5; i++)
    {
      await Sender.Send(new AddCandidateEntryCommand
      {
        DraftPartId = draftPartPublicId,
        TmdbId = 1_000_000 + i,
        AddedByPublicId = addedByPublicId
      });
    }

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 1,
      PageSize = 2
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Value.Response.TotalCount.Should().Be(5);
    result.Value.Response.Items.Should().HaveCount(2);
    result.Value.Response.Page.Should().Be(1);
    result.Value.Response.PageSize.Should().Be(2);
  }

  [Fact]
  public async Task GetCandidateList_ShouldReturnSecondPage_WhenPaginatingAsync()
  {
    // Arrange
    var draftPartPublicId = await SetupDraftPartAsync();
    var addedByPublicId = "u_" + Faker.Random.AlphaNumeric(17);

    for (var i = 0; i < 4; i++)
    {
      await Sender.Send(new AddCandidateEntryCommand
      {
        DraftPartId = draftPartPublicId,
        TmdbId = 2_000_000 + i,
        AddedByPublicId = addedByPublicId
      });
    }

    var query = new GetCandidateListQuery
    {
      DraftPartId = draftPartPublicId,
      Page = 2,
      PageSize = 2
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.Value.Response.TotalCount.Should().Be(4);
    result.Value.Response.Items.Should().HaveCount(2);
    result.Value.Response.Page.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<string> SetupDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAndPartAsync(seriesId);
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));

    return draftPart.PublicId;
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    var result = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName() + Faker.Random.AlphaNumeric(6),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });

    return result.Value;
  }

  private async Task<string> CreateDraftAndPartAsync(Guid seriesId)
  {
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    });

    var draftPublicId = draftResult.Value;

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    return draftPublicId;
  }
}
