using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Features.Drafts.GetDraft;
using ScreenDrafts.Modules.Drafts.Features.DraftParts.SetReleaseDate;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Drafts;

public sealed class GetDraftTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // ─────────────────────────────────────────────────────────────────────────
  // Not found
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithNonExistentId_ShouldReturnFailureAsync()
  {
    // Arrange
    var query = new GetDraftQuery { DraftId = Faker.Random.AlphaNumeric(10), IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsFailure.Should().BeTrue();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Draft root fields
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_ShouldReturnCorrectDraftFieldsAsync()
  {
    // Arrange
    const string title = "My Integration Test Draft";
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = (await Sender.Send(new CreateDraftCommand
    {
      Title = title,
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    })).Value;

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.PublicId.Should().Be(draftPublicId);
    result.Value.Title.Should().Be(title);
    result.Value.DraftType.Should().Be(DraftType.Standard.Value);
    result.Value.SeriesPublicId.Should().NotBeNullOrEmpty();
    result.Value.CampaignPublicId.Should().BeNull();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Draft parts
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithNoParts_ShouldReturnEmptyPartsListAsync()
  {
    // Arrange
    var draftPublicId = await CreateDraftAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraft_WithOnePart_ShouldReturnOnePartAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Should().HaveCount(1);
    result.Value.Parts[0].PartIndex.Should().Be(1);
  }

  [Fact]
  public async Task GetDraft_WithMultipleParts_ShouldReturnPartsOrderedByIndexAsync()
  {
    // Arrange
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = (await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    })).Value;

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

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Should().HaveCount(2);
    result.Value.Parts[0].PartIndex.Should().Be(1);
    result.Value.Parts[1].PartIndex.Should().Be(2);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Hosts
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithPrimaryHost_ShouldPopulateHostAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftPublicId);

    var peopleFactory = new PeopleFactory(Sender, Faker);
    var personId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = personId })).Value;

    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var part = result.Value.Parts.Single();
    part.PrimaryHost.Should().NotBeNull();
    part.PrimaryHost!.HostPublicId.Should().Be(hostPublicId);
  }

  [Fact]
  public async Task GetDraft_WithNoHost_PrimaryHostShouldBeNullAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().PrimaryHost.Should().BeNull();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Participants
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithParticipant_ShouldPopulateParticipantsAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync();
    var teamFactory = new DrafterTeamFactory(Sender, Faker);
    var drafterPublicId = await teamFactory.CreateAndSaveDrafterAsync();

    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var part = result.Value.Parts.Single();
    part.Participants.Should().HaveCount(1);
    part.Participants[0].ParticipantKindValue.Should().Be(ParticipantKind.Drafter.Value);
  }

  [Fact]
  public async Task GetDraft_WithNoParticipants_ParticipantsShouldBeEmptyAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().Participants.Should().BeEmpty();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Releases
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithRelease_ShouldPopulateReleasesAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync();
    var releaseDate = new DateOnly(2025, 6, 15);

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = releaseDate,
      ReleaseChannel = ReleaseChannel.MainFeed
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var part = result.Value.Parts.Single();
    part.Releases.Should().HaveCount(1);
    part.Releases[0].ReleaseDate.Should().Be(releaseDate);
    part.Releases[0].ReleaseChannel.Should().Be(ReleaseChannel.MainFeed.Value);
  }

  [Fact]
  public async Task GetDraft_WithNoRelease_ReleasesShouldBeEmptyAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().Releases.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraft_ExcludesPatreonReleases_WhenIncludePatreonIsFalseAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync();

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = new DateOnly(2025, 6, 15),
      ReleaseChannel = ReleaseChannel.Patreon
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().Releases.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraft_IncludesPatreonReleases_WhenIncludePatreonIsTrueAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId) = await CreateDraftWithPartAsync();

    await Sender.Send(new SetReleaseDateCommand
    {
      DraftPartId = draftPartPublicId,
      ReleaseDate = new DateOnly(2025, 6, 15),
      ReleaseChannel = ReleaseChannel.Patreon
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = true };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().Releases.Should().HaveCount(1);
    result.Value.Parts.Single().Releases[0].ReleaseChannel.Should().Be(ReleaseChannel.Patreon.Value);
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Picks
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithNoPicks_PicksShouldBeEmptyAsync()
  {
    // Arrange
    var (draftPublicId, _) = await CreateDraftWithPartAsync();
    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Parts.Single().Picks.Should().BeEmpty();
  }

  [Fact]
  public async Task GetDraft_WithPick_ShouldPopulatePicksAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var part = result.Value.Parts.Single();
    part.Picks.Should().HaveCount(1);
    var pick = part.Picks[0];
    pick.PlayOrder.Should().Be(1);
    pick.Position.Should().Be(1);
    pick.MoviePublicId.Should().Be(movie.ImdbId);
    pick.MovieTitle.Should().Be(movie.MovieTitle);
    pick.Veto.Should().BeNull();
    pick.CommissionerOverride.Should().BeNull();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Vetoes
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithVeto_ShouldPopulateVetoOnPickAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Parts.Single().Picks.Single();
    pick.Veto.Should().NotBeNull();
    pick.Veto!.IsOverriden.Should().BeFalse();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Commissioner Overrides
  // ─────────────────────────────────────────────────────────────────────────

  [Fact]
  public async Task GetDraft_WithCommissionerOverride_ShouldPopulateOverrideAsync()
  {
    // Arrange
    var (draftPublicId, draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();

    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = 1,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });

    await Sender.Send(new ApplyCommissionerOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1
    });

    var query = new GetDraftQuery { DraftId = draftPublicId, IncludePatreon = false };

    // Act
    var result = await Sender.Send(query);

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Parts.Single().Picks.Single();
    pick.CommissionerOverride.Should().NotBeNull();
  }

  // ─────────────────────────────────────────────────────────────────────────
  // Helpers
  // ─────────────────────────────────────────────────────────────────────────

  private async Task<string> CreateDraftAsync()
  {
    var seriesId = await CreateSeriesAsync();
    return (await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    })).Value;
  }

  private async Task<(string DraftPublicId, string DraftPartPublicId)> CreateDraftWithPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = (await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    })).Value;

    var partPublicId = (await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    })).Value;

    return (draftPublicId, partPublicId);
  }

  private async Task<Guid> CreateSeriesAsync()
  {
    return (await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    })).Value;
  }

  private async Task<(string DraftPublicId, string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)> SetupStartedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = (await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesId
    })).Value;

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftPublicId,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7
    });

    var draftPartInternalId = await GetFirstDraftPartIdAsync(draftPublicId);

    var draftPart = await DbContext.DraftParts
      .FirstAsync(dp => dp.Id == DraftPartId.Create(draftPartInternalId));
    var draftPartPublicId = draftPart.PublicId;

    var peopleFactory = new PeopleFactory(Sender, Faker);

    var person1Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter1PublicId = (await Sender.Send(new CreateDrafterCommand(person1Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var person2Id = await peopleFactory.CreateAndSavePersonAsync();
    var drafter2PublicId = (await Sender.Send(new CreateDrafterCommand(person2Id))).Value;
    await Sender.Send(new AddParticipantToDraftPartCommand
    {
      DraftPartPublicId = draftPartPublicId,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter
    });

    var hostPersonId = await peopleFactory.CreateAndSavePersonAsync();
    var hostPublicId = (await Sender.Send(new CreateHostCommand { PersonPublicId = hostPersonId })).Value;
    await Sender.Send(new AddHostToDraftPartCommand
    {
      DraftPartId = draftPartPublicId,
      HostPublicId = hostPublicId,
      HostRole = HostRole.Primary
    });

    await Sender.Send(new SetDraftPartStatusCommand
    {
      SetDraftPartStatusRequest = new SetDraftPartStatusRequest
      {
        DraftPublicId = draftPublicId,
        PartIndex = 1,
        Action = DraftPartStatusAction.Start
      }
    });

    return (draftPublicId, draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync();
    return movie;
  }
}
