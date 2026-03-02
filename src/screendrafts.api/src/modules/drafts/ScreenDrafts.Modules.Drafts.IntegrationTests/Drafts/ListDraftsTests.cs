using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.Drafts.ListDrafts;
using ScreenDrafts.Modules.Drafts.Features.Drafts.SetCategories;
using ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class ListDraftsTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ─────────────────────────────────────────────────────────────────────────
  // Empty list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithNoDrafts_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
    result.Value.TotalCount.Should().Be(0);
    result.Value.Page.Should().Be(1);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Basic list
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithOneDraftPart_ShouldReturnOneItemAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.TotalCount.Should().Be(1);
  }

  [Fact]
  public async Task ListDrafts_WithMultipleDraftParts_ShouldReturnAllAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    await CreateDraftWithPartAsync();
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.TotalCount.Should().Be(3);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Response shape
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_ShouldPopulatePublicIdsAndLabelAsync()
  {
    // Arrange
    const string title = "Test Draft Title";
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync(title: title);
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.DraftPublicId.Should().Be(draftPublicId);
    item.DraftPartPublicId.Should().Be(draftPartPublicId);
    item.Label.Should().Be(title); // single-part draft: label == title
  }

  [Fact]
  public async Task ListDrafts_MultiPartDraft_LabelShouldIncludePartIndexAsync()
  {
    // Arrange: create a draft, then add a second part manually so COUNT(*) > 1
    var seriesId = await CreateSeriesAsync();
    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = "Multi-Part Draft",
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    });
    var draftPublicId = draftResult.Value;

    // First part (auto-created by CreateDraft is not created here — we explicitly create parts)
    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 2,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      SortBy = "title",
      Dir = "asc"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);

    var labels = result.Value.Items.Select(i => i.Label).ToList();
    labels.Should().Contain(l => l.Contains("Part 1"));
    labels.Should().Contain(l => l.Contains("Part 2"));
    labels.Should().AllSatisfy(l => l.Should().StartWith("Multi-Part Draft"));
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Pagination
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_Pagination_ShouldReturnCorrectPageAsync()
  {
    // Arrange – create 5 parts, request page 2 with page-size 2
    for (var i = 0; i < 5; i++)
    {
      await CreateDraftWithPartAsync();
    }

    var query = new ListDraftsQuery { Page = 2, PageSize = 2 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.TotalCount.Should().Be(5);
    result.Value.Page.Should().Be(2);
    result.Value.TotalPages.Should().Be(3);
    result.Value.HasPreviousPage.Should().BeTrue();
    result.Value.HasNextPage.Should().BeTrue();
  }

  [Fact]
  public async Task ListDrafts_PageSizeCappedAt100Async()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 1000 }; // exceeds cap

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PageSize.Should().Be(100);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Releases
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithRelease_ShouldPopulateReleasesAsync()
  {
    // Arrange
    var (_, draftPartPublicId) = await CreateDraftWithPartAsync();
    var releaseDate = new DateOnly(2025, 6, 15);

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = releaseDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.Releases.Should().HaveCount(1);
    item.Releases[0].ReleaseDate.Should().Be(releaseDate);
    item.Releases[0].ReleaseChannel.Should().Be(ReleaseChannel.MainFeed.Value);
  }

  [Fact]
  public async Task ListDrafts_WithNoRelease_ReleasesListShouldBeEmptyAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Single().Releases.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Participants
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithParticipant_ShouldPopulateParticipantsAsync()
  {
    // Arrange
    var (_, draftPartPublicId) = await CreateDraftWithPartAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.Participants.Should().HaveCount(1);
    item.Participants[0].ParticipantKindValue.Should().Be(ParticipantKind.Drafter.Value);
  }

  [Fact]
  public async Task ListDrafts_WithNoParticipants_ParticipantsListShouldBeEmptyAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Single().Participants.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Hosts
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithHost_ShouldPopulateHostsAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartInternalId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var item = result.Value.Items.Single();
    item.Hosts.Should().HaveCount(1);
    item.Hosts[0].HostPublicId.Should().Be(hostPublicId);
  }

  [Fact]
  public async Task ListDrafts_WithNoHosts_HostsListShouldBeEmptyAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync();
    var query = new ListDraftsQuery { Page = 1, PageSize = 10 };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Single().Hosts.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Category filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithCategoryFilter_ShouldReturnOnlyMatchingDraftsAsync()
  {
    // Arrange – two drafts; only the first is assigned to the target category
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    await CreateDraftWithPartAsync(); // unrelated draft

    var categoryPublicId = await CreateCategoryAsync();
    await Sender.Send(new SetCategoriesDraftCommand
    {
      DraftId = draftPublicId,
      CategoryIds = [categoryPublicId]
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      CategoryPublicId = categoryPublicId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftPublicId.Should().Be(draftPublicId);
  }

  [Fact]
  public async Task ListDrafts_WithCategoryFilter_NoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange – draft exists but is not assigned to the queried category
    await CreateDraftWithPartAsync();
    var unusedCategoryPublicId = await CreateCategoryAsync();

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      CategoryPublicId = unusedCategoryPublicId
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Draft type filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithDraftTypeFilter_ShouldReturnOnlyMatchingTypeAsync()
  {
    // Arrange – one Standard part and one MiniMega part
    await CreateDraftWithPartAsync(draftType: DraftType.Standard);
    await CreateDraftWithPartAsync(draftType: DraftType.MiniMega);

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      DraftType = DraftType.Standard.Value
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftType.Should().Be(DraftType.Standard.Value);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Search query (title)
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithSearchQuery_ShouldFilterByTitleAsync()
  {
    // Arrange
    const string uniqueTitle = "UniqueSearchKeyword_XYZ";
    await CreateDraftWithPartAsync(title: uniqueTitle);
    await CreateDraftWithPartAsync(title: "SomethingCompletelyDifferent");

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      Q = "UniqueSearch"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().Label.Should().Contain(uniqueTitle);
  }

  [Fact]
  public async Task ListDrafts_WithSearchQuery_NoMatch_ShouldReturnEmptyAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync(title: "Regular Draft Title");

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      Q = "DefinitelyNoSuchTitle_AbsolutelyNotExists"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Date range filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithFromDateFilter_ShouldExcludePartsReleasedBeforeAsync()
  {
    // Arrange – two parts: one released before filter date, one after
    var (_, earlyPartPublicId) = await CreateDraftWithPartAsync();
    var (_, latePartPublicId) = await CreateDraftWithPartAsync();

    var earlyDate = new DateOnly(2024, 1, 1);
    var lateDate = new DateOnly(2025, 6, 1);
    var filterDate = new DateOnly(2024, 6, 1);

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = earlyPartPublicId,
      ReleaseDate = earlyDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = latePartPublicId,
      ReleaseDate = lateDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      FromDate = filterDate
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftPartPublicId.Should().Be(latePartPublicId);
  }

  [Fact]
  public async Task ListDrafts_WithToDateFilter_ShouldExcludePartsReleasedAfterAsync()
  {
    // Arrange – two parts: one released before filter date, one after
    var (_, earlyPartPublicId) = await CreateDraftWithPartAsync();
    var (_, latePartPublicId) = await CreateDraftWithPartAsync();

    var earlyDate = new DateOnly(2024, 1, 1);
    var lateDate = new DateOnly(2025, 6, 1);
    var filterDate = new DateOnly(2024, 6, 1);

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = earlyPartPublicId,
      ReleaseDate = earlyDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = latePartPublicId,
      ReleaseDate = lateDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      ToDate = filterDate
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftPartPublicId.Should().Be(earlyPartPublicId);
  }

  [Fact]
  public async Task ListDrafts_WithFromAndToDate_ShouldReturnOnlyPartsInRangeAsync()
  {
    // Arrange – three parts with different release dates
    var (_, jan2024PublicId) = await CreateDraftWithPartAsync();
    var (_, jun2024PublicId) = await CreateDraftWithPartAsync();
    var (_, jan2025PublicId) = await CreateDraftWithPartAsync();

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = jan2024PublicId,
      ReleaseDate = new DateOnly(2024, 1, 1),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = jun2024PublicId,
      ReleaseDate = new DateOnly(2024, 6, 15),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = jan2025PublicId,
      ReleaseDate = new DateOnly(2025, 1, 1),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      FromDate = new DateOnly(2024, 3, 1),
      ToDate = new DateOnly(2024, 12, 31)
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftPartPublicId.Should().Be(jun2024PublicId);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Drafter count filter
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_WithMinDraftersFilter_ShouldReturnOnlyPartsWithEnoughDraftersAsync()
  {
    // Arrange – one part with 2 drafters, one with 0
    var (_, richPartPublicId) = await CreateDraftWithPartAsync();
    await CreateDraftWithPartAsync();

    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafter1 = await teamFactory.CreateAndSaveDrafterAsync();
    var drafter2 = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = richPartPublicId,
      ParticipantPublicId = drafter1,
      ParticipantKind = ParticipantKind.Drafter
    });

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = richPartPublicId,
      ParticipantPublicId = drafter2,
      ParticipantKind = ParticipantKind.Drafter
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      MinDrafters = 2,
      MaxDrafters = 10
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(1);
    result.Value.Items.Single().DraftPartPublicId.Should().Be(richPartPublicId);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Sorting
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task ListDrafts_SortedByTitle_AscShouldReturnInAlphabeticOrderAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync(title: "Zebra Draft");
    await CreateDraftWithPartAsync(title: "Alpha Draft");
    await CreateDraftWithPartAsync(title: "Middle Draft");

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      SortBy = "title",
      Dir = "asc"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(3);
    result.Value.Items[0].Label.Should().Be("Alpha Draft");
    result.Value.Items[1].Label.Should().Be("Middle Draft");
    result.Value.Items[2].Label.Should().Be("Zebra Draft");
  }

  [Fact]
  public async Task ListDrafts_SortedByTitle_DescShouldReturnInReverseOrderAsync()
  {
    // Arrange
    await CreateDraftWithPartAsync(title: "Zebra Draft");
    await CreateDraftWithPartAsync(title: "Alpha Draft");
    await CreateDraftWithPartAsync(title: "Middle Draft");

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      SortBy = "title",
      Dir = "desc"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items[0].Label.Should().Be("Zebra Draft");
    result.Value.Items[2].Label.Should().Be("Alpha Draft");
  }

  [Fact]
  public async Task ListDrafts_SortedByDate_ShouldReturnInReleaseDateOrderAsync()
  {
    // Arrange – two parts with known release dates
    var (_, olderPartPublicId) = await CreateDraftWithPartAsync(title: "Older Draft");
    var (_, newerPartPublicId) = await CreateDraftWithPartAsync(title: "Newer Draft");

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = olderPartPublicId,
      ReleaseDate = new DateOnly(2023, 1, 1),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = newerPartPublicId,
      ReleaseDate = new DateOnly(2024, 1, 1),
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new ListDraftsQuery
    {
      Page = 1,
      PageSize = 10,
      SortBy = "date",
      Dir = "asc"
    };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Items.Should().HaveCount(2);
    result.Value.Items[0].DraftPartPublicId.Should().Be(olderPartPublicId);
    result.Value.Items[1].DraftPartPublicId.Should().Be(newerPartPublicId);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task<(string DraftPublicId, string DraftPartPublicId)> CreateDraftWithPartAsync(
    string? title = null,
    DraftType? draftType = null)
  {
    var seriesId = await CreateSeriesAsync();
    var selectedDraftType = draftType ?? DraftType.Standard;
    var draftTitle = title ?? Faker.Company.CompanyName();

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = draftTitle,
      DraftType = selectedDraftType.Value,
      SeriesId = seriesId
    });

    var draftPublicId = draftResult.Value;

    var partResult = await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    return (draftPublicId, partResult.Value);
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

  private async Task<string> CreateCategoryAsync()
  {
    var result = await Sender.Send(new CreateCategoryCommand
    {
      Name = Faker.Commerce.Categories(1)[0] + Faker.Random.AlphaNumeric(8),
      Description = Faker.Lorem.Sentence()
    });

    return result.Value;
  }
}
