namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts;

public sealed class GetPickListTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Empty list
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WithNoPicks_ShouldReturnEmptyListAsync()
  {
    // Arrange
    var (draftPartPublicId, _, _) = await SetupStartedDraftPartAsync();

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().BeEmpty();
  }

  [Fact]
  public async Task GetPickList_WithNonExistentDraftPartId_ShouldReturnEmptyListAsync()
  {
    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = Faker.Random.AlphaNumeric(12) });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().BeEmpty();
  }

  // -------------------------------------------------------------------------
  // Single pick — response shape
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WithOnePick_ShouldReturnOneItemAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().HaveCount(1);
  }

  [Fact]
  public async Task GetPickList_ShouldPopulatePickFieldsAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 3, playOrder: 1, movie);

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single();
    pick.PlayOrder.Should().Be(1);
    pick.Position.Should().Be(3);
    pick.MovieImdbId.Should().Be(movie.ImdbId);
    pick.MovieTitle.Should().Be(movie.MovieTitle);
    pick.PlayedByParticipantKindValue.Should().Be(ParticipantKind.Drafter.Value);
    pick.Veto.Should().BeNull();
    pick.HasCommissionerOverride.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Multiple picks — ordering
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WithMultiplePicks_ShouldReturnAllPicksOrderedByPlayOrderAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie1 = await CreateMovieAsync();
    var movie2 = await CreateMovieAsync();
    var movie3 = await CreateMovieAsync();

    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie1);
    await PlayPickAsync(draftPartPublicId, drafter2PublicId, position: 2, playOrder: 2, movie2);
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 3, playOrder: 3, movie3);

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().HaveCount(3);
    result.Value.Picks[0].PlayOrder.Should().Be(1);
    result.Value.Picks[1].PlayOrder.Should().Be(2);
    result.Value.Picks[2].PlayOrder.Should().Be(3);
  }

  // -------------------------------------------------------------------------
  // Veto — applied, not overridden
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WhenPickIsVetoed_ShouldPopulateVetoAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single();
    pick.Veto.Should().NotBeNull();
    pick.Veto!.IsOverridden.Should().BeFalse();
    pick.Veto.Override.Should().BeNull();
    pick.Veto.IssuedByParticipantKindValue.Should().Be(ParticipantKind.Drafter.Value);
  }

  [Fact]
  public async Task GetPickList_WhenPickIsNotVetoed_VetoShouldBeNullAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie1 = await CreateMovieAsync();
    var movie2 = await CreateMovieAsync();

    // Play two picks; veto only the second
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie1);
    await PlayPickAsync(draftPartPublicId, drafter2PublicId, position: 2, playOrder: 2, movie2);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    });

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Should().HaveCount(2);
    result.Value.Picks[0].Veto.Should().BeNull();
    result.Value.Picks[1].Veto.Should().NotBeNull();
  }

  // -------------------------------------------------------------------------
  // Veto override
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WhenVetoIsOverridden_ShouldPopulateOverrideAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantPublicId = drafter1PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter1PublicId
    });

    await Sender.Send(new ApplyVetoOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1,
      ParticipantIdValue = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    });

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single();
    pick.Veto.Should().NotBeNull();
    pick.Veto!.IsOverridden.Should().BeTrue();
    pick.Veto.Override.Should().NotBeNull();
    pick.Veto.Override!.IssuedByParticipantKindValue.Should().Be(ParticipantKind.Drafter.Value);
  }

  // -------------------------------------------------------------------------
  // Commissioner override
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_WhenPickHasCommissionerOverride_ShouldSetFlagAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    await Sender.Send(new ApplyCommissionerOverrideCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 1
    });

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var pick = result.Value.Picks.Single();
    pick.HasCommissionerOverride.Should().BeTrue();
    pick.Veto.Should().BeNull();
  }

  [Fact]
  public async Task GetPickList_WhenPickHasNoCommissionerOverride_FlagShouldBeFalseAsync()
  {
    // Arrange
    var (draftPartPublicId, drafter1PublicId, _) = await SetupStartedDraftPartAsync();
    var movie = await CreateMovieAsync();
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie);

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Picks.Single().HasCommissionerOverride.Should().BeFalse();
  }

  // -------------------------------------------------------------------------
  // Mixed: veto + commissioner override on separate picks
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetPickList_MixedOverrides_ShouldReflectEachPickCorrectlyAsync()
  {
    // Arrange — pick 1: commissioner override; pick 2: veto; pick 3: plain
    var (draftPartPublicId, drafter1PublicId, drafter2PublicId) = await SetupStartedDraftPartAsync();
    var movie1 = await CreateMovieAsync();
    var movie2 = await CreateMovieAsync();
    var movie3 = await CreateMovieAsync();

    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 1, playOrder: 1, movie1);
    await PlayPickAsync(draftPartPublicId, drafter2PublicId, position: 2, playOrder: 2, movie2);
    await PlayPickAsync(draftPartPublicId, drafter1PublicId, position: 3, playOrder: 3, movie3);

    await Sender.Send(new ApplyCommissionerOverrideCommand { DraftPartId = draftPartPublicId, PlayOrder = 1 });
    await Sender.Send(new ApplyVetoCommand
    {
      DraftPartId = draftPartPublicId,
      PlayOrder = 2,
      ParticipantPublicId = drafter2PublicId,
      ParticipantKind = ParticipantKind.Drafter,
      ActorPublicId = drafter2PublicId
    });

    // Act
    var result = await Sender.Send(new GetPickListQuery { DraftPartId = draftPartPublicId });

    // Assert
    result.IsSuccess.Should().BeTrue();
    var picks = result.Value.Picks;
    picks.Should().HaveCount(3);

    picks[0].HasCommissionerOverride.Should().BeTrue();
    picks[0].Veto.Should().BeNull();

    picks[1].HasCommissionerOverride.Should().BeFalse();
    picks[1].Veto.Should().NotBeNull();
    picks[1].Veto!.IsOverridden.Should().BeFalse();

    picks[2].HasCommissionerOverride.Should().BeFalse();
    picks[2].Veto.Should().BeNull();
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string Drafter1PublicId, string Drafter2PublicId)> SetupStartedDraftPartAsync()
  {
    var seriesId = await CreateSeriesAsync();
    var draftPublicId = await CreateDraftAsync(seriesId);
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

    return (draftPartPublicId, drafter1PublicId, drafter2PublicId);
  }

  private async Task PlayPickAsync(string draftPartPublicId, string drafterPublicId, int position, int playOrder, Movie movie)
  {
    await Sender.Send(new PlayPickCommand
    {
      DraftPartId = draftPartPublicId,
      Position = position,
      PlayOrder = playOrder,
      ParticipantPublicId = drafterPublicId,
      ParticipantKind = ParticipantKind.Drafter,
      MovieId = movie.Id
    });
  }

  private async Task<Movie> CreateMovieAsync()
  {
    var movie = Movie.Create(Faker.Company.CompanyName(), Faker.Random.AlphaNumeric(10), Guid.NewGuid()).Value;
    DbContext.Movies.Add(movie);
    await DbContext.SaveChangesAsync();
    return movie;
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
