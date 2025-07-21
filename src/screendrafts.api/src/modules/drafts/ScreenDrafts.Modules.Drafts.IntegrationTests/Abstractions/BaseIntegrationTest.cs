namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Reviewed")]
public class BaseIntegrationTest : IDisposable, IAsyncLifetime
{
  private bool _disposedValue;
  protected static readonly Faker Faker = new();
  private readonly IServiceScope _serviceScope;
  protected readonly ISender Sender;
  protected readonly HttpClient HttpClient;
  protected readonly DraftsDbContext DbContext;

  public BaseIntegrationTest(IntegrationTestWebAppFactory factory)
  {
    ArgumentNullException.ThrowIfNull(factory);

    _serviceScope = factory.Services.CreateScope();
    Sender = _serviceScope.ServiceProvider.GetRequiredService<ISender>();
    HttpClient = factory.CreateClient();
    DbContext = _serviceScope.ServiceProvider.GetRequiredService<DraftsDbContext>();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (!_disposedValue)
    {
      if (disposing)
      {
        _serviceScope.Dispose();
        HttpClient.Dispose();
        DbContext.Dispose();
      }

      _disposedValue = true;
    }
  }

  public async Task InitializeAsync()
  {
    await ClearDatabaseAsync();
  }

  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }

  public async Task DisposeAsync()
  {
    await ClearDatabaseAsync();
    Dispose();
  }

  public async Task<(Result<Guid> draftId, List<Drafter> drafters, List<Host> hosts)> SetupDraftAndDraftersAsync(DraftType draftType)
  {
    ArgumentNullException.ThrowIfNull(draftType);

    Draft? draft = null!;
    switch (draftType.Value)
    {
      case 0:
        draft = DraftFactory.CreateStandardDraft().Value;
        break;
      case 1:
        draft = DraftFactory.CreateMiniMegaDraft().Value;
        break;
      case 2:
        draft = DraftFactory.CreateMegaDraft().Value;
        break;
      default:
        break;
    }
    var draftId = await Sender.Send(new CreateDraftCommand(
      draft.Title.Value,
      draft.DraftType,
      draft.TotalPicks,
      draft.TotalDrafters,
      draft.TotalDrafterTeams,
      draft.TotalHosts,
      draft.EpisodeType,
      draft.DraftStatus));

    var drafters = new List<Drafter>();
    var hosts = new List<Host>();

    for (var i = 0; i < draft.TotalDrafters; i++)
    {
      var drafterFactory = new DrafterFactory(Sender, Faker);
      var drafterId = await drafterFactory.CreateAndSaveDrafterAsync();
      var addedDrafterId = await Sender.Send(new AddDrafterToDraftCommand(
        draftId.Value,
        drafterId));
      var addedDrafter = await Sender.Send(new GetDrafterQuery(addedDrafterId.Value));

      var addedPerson = await Sender.Send(new GetPersonQuery(addedDrafter.Value.PersonId));
      var addedPersonResult = Domain.People.Person.Create(
        addedPerson.Value.FirstName,
        addedPerson.Value.LastName,
        id: addedDrafter.Value.PersonId);
      drafters.Add(Drafter.Create(
        person: addedPersonResult.Value,
        id: DrafterId.Create(addedDrafter.Value.Id)).Value);
    }

    for (var i = 0; i < draft.TotalHosts; i++)
    {
      var hostFactory = new HostsFactory(Sender, Faker);
      var hostId = await hostFactory.CreateAndSaveHostAsync();
      var addedHostId = await Sender.Send(new AddHostToDraftCommand(
        draftId.Value,
        hostId));
      var addedHost = await Sender.Send(new GetHostQuery(addedHostId.Value));

      var addedPerson = await Sender.Send(new GetPersonQuery(addedHost.Value.PersonId));
      var addedPersonResult = Domain.People.Person.Create(
        addedPerson.Value.FirstName,
        addedPerson.Value.LastName,
        id: addedHost.Value.PersonId);

      hosts.Add(Host.Create(
        addedPersonResult.Value,
        id: HostId.Create(addedHost.Value.Id)).Value);
    }

    Collection<DraftPosition> draftPositions = [];
    if (draftType.Value == 1)
    {
      draftPositions = DraftFactory.CreateMiniMegaDraftPositions();
    }
    else if (draftType.Value == 2)
    {
      draftPositions = DraftFactory.CreateMegaDraftPositions();
    }
    var draftPositionsRequests = new Collection<DraftPositionRequest>(
      [.. draftPositions.Select(dp => new DraftPositionRequest(
        dp.Name,
        dp.Picks,
        dp.HasBonusVeto,
        dp.HasBonusVetoOverride))]);

    var gameBoardId = await Sender.Send(new CreateGameBoardCommand(
      draftId.Value));

    await Sender.Send(new AddDraftPositionsToGameBoardCommand(gameBoardId.Value, draftPositionsRequests));


    var query = new GetDraftPositionsByGameBoardQuery(gameBoardId.Value);
    var draftPositionsResponse = await Sender.Send(query);
    var draftPositionsList = draftPositionsResponse.Value.ToList();

    for (var i = 0; i < draft.TotalDrafters; i++)
    {
      var drafterId = drafters[i].Id;
      var draftPositionId = draftPositionsList[i].Id;
      var command = new AssignDraftPositionCommand(
        draftId.Value,
        drafterId.Value,
        draftPositionId);
      await Sender.Send(command);
    }

    return (draftId, drafters, hosts);
  }

  private async Task ClearDatabaseAsync()
  {
    await DbContext.Database.ExecuteSqlRawAsync(
      $"""
      TRUNCATE TABLE 
        drafts.people,
        drafts.drafts,
        drafts.drafters,
        drafts.hosts,
        drafts.drafter_draft_stats,
        drafts.draft_positions,
        drafts.picks,
        drafts.game_boards,
        drafts.drafts_drafters,
        drafts.draft_host,
        drafts.draft_release_date,
        drafts.movies,
        drafts.trivia_results,
        drafts.rollover_veto_overrides,
        drafts.rollover_vetoes,
        drafts.vetoes,
        drafts.veto_overrides
      RESTART IDENTITY CASCADE;
      """);
  }
}
