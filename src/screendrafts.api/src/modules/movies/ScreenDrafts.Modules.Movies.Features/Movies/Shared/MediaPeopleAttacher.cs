namespace ScreenDrafts.Modules.Movies.Features.Movies.Shared;

/// <summary>
/// Attaches people and production companies to an existing Media aggregate.
/// Used by both AddMediaCommandHandler and SyncMediaPeopleCommandHandler.
/// </summary>
internal sealed class MediaPeopleAttacher(
  IMediaRepository mediaRepository,
  IPersonRepository personRepository,
  IProductionCompanyRepository productionCompanyRepository
)
{
  private readonly IMediaRepository _mediaRepository = mediaRepository;
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IProductionCompanyRepository _productionCompanyRepository =
    productionCompanyRepository;

  public async Task AttachAsync(
    Media media,
    IReadOnlyCollection<PersonRequest>? directors,
    IReadOnlyCollection<PersonRequest>? actors,
    IReadOnlyCollection<PersonRequest>? writers,
    IReadOnlyCollection<PersonRequest>? producers,
    IReadOnlyCollection<ProductionCompanyRequest>? productionCompanies,
    CancellationToken cancellationToken
  )
  {
    var peopleCache = new Dictionary<int, Person>();
    var addedDirectors = new HashSet<Guid>();
    var addedActors = new HashSet<Guid>();
    var addedWriters = new HashSet<Guid>();
    var addedProducers = new HashSet<Guid>();

    async Task<Person?> GetOrCreatePersonAsync(string name, string? imdbId, int tmdbId)
    {
      if (peopleCache.TryGetValue(tmdbId, out var cached))
      {
        return cached;
      }

      // Prefer ImdbId lookup to avoid duplicates with records seeded under the old IMDb-first path.
      Person? person = !string.IsNullOrWhiteSpace(imdbId)
        ? await _personRepository.FindByImdbIdAsync(imdbId, cancellationToken)
        : null;

      person ??= await _personRepository.FindByTmdbAsync(tmdbId, cancellationToken);

      if (person is null)
      {
        person = Person.Create(imdbId, name, tmdbId);
        _personRepository.Add(person);
      }

      peopleCache[tmdbId] = person;
      return person;
    }

    if (directors is not null)
    {
      foreach (var d in directors)
      {
        var person = await GetOrCreatePersonAsync(d.Name, d.ImdbId, d.TmdbId);
        if (person is not null && addedDirectors.Add(person.Id.Value))
        {
          _mediaRepository.AddMediaDirector(media, person);
        }
      }
    }

    if (actors is not null)
    {
      foreach (var a in actors)
      {
        var person = await GetOrCreatePersonAsync(a.Name, a.ImdbId, a.TmdbId);
        if (person is not null && addedActors.Add(person.Id.Value))
        {
          _mediaRepository.AddMediaActor(media, person);
        }
      }
    }

    if (writers is not null)
    {
      foreach (var w in writers)
      {
        var person = await GetOrCreatePersonAsync(w.Name, w.ImdbId, w.TmdbId);
        if (person is not null && addedWriters.Add(person.Id.Value))
        {
          _mediaRepository.AddMediaWriter(media, person);
        }
      }
    }

    if (producers is not null)
    {
      foreach (var p in producers)
      {
        var person = await GetOrCreatePersonAsync(p.Name, p.ImdbId, p.TmdbId);
        if (person is not null && addedProducers.Add(person.Id.Value))
        {
          _mediaRepository.AddMediaProducer(media, person);
        }
      }
    }

    if (productionCompanies is not null)
    {
      var companiesCache = new Dictionary<int, ProductionCompany>();

      foreach (var company in productionCompanies)
      {
        if (!companiesCache.TryGetValue(company.TmdbId, out var productionCompany))
        {
          productionCompany = await _productionCompanyRepository.FindByTmdbIdAsync(
            company.TmdbId,
            cancellationToken
          );

          if (productionCompany is null)
          {
            productionCompany = ProductionCompany.Create(
              company.Name,
              company.ImdbId,
              company.TmdbId
            );
            _productionCompanyRepository.Add(productionCompany);
          }

          companiesCache[company.TmdbId] = productionCompany;
        }

        if (
          !await _productionCompanyRepository.RelationshipExistsAsync(
            media.Id.Value,
            productionCompany.Id,
            cancellationToken
          )
        )
        {
          _mediaRepository.AddMediaProductionCompany(media, productionCompany);
        }
      }
    }
  }
}
