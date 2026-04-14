namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Fixtures;

/// <summary>
/// Seeds all Media (Movie) records required by the scenario tests.
/// Deduplicates by IMDb ID — films that appear in multiple scenarios
/// (Raging Bull tt0081398, The Color of Money tt0090863) are inserted once.
///
/// Call <see cref="SeedAsync"/> once from each scenario test class's InitializeAsync.
/// Returns a dictionary keyed by IMDb ID (or generated key for no-IMDb films).
/// </summary>
public sealed class MediaSeedFixture
{
  private readonly Dictionary<string, string> _publicIdByKey = new(StringComparer.OrdinalIgnoreCase);
  private bool _seeded;

  /// <summary>Get the movie publicId for a given IMDb ID or title key.</summary>
  public string this[string key] => _publicIdByKey[key];

  public bool TryGet(string key, out string publicId) =>
    _publicIdByKey.TryGetValue(key, out publicId!);

  public async Task SeedAsync(DraftsDbContext dbContext)
  {
    ArgumentNullException.ThrowIfNull(dbContext);
    if (_seeded) return;

    // ── Scenario 3: Aaron Sorkin Super Draft ──────────────────────────────────
    await SeedMovieAsync(dbContext, "A Few Good Men", "tt0104257");
    await SeedMovieAsync(dbContext, "Malice", "tt0107497");
    await SeedMovieAsync(dbContext, "The American President", "tt0112346");
    await SeedMovieAsync(dbContext, "Charlie Wilson's War", "tt0472062");
    await SeedMovieAsync(dbContext, "The Social Network", "tt1285016");
    await SeedMovieAsync(dbContext, "Moneyball", "tt1210166");
    await SeedMovieAsync(dbContext, "Steve Jobs", "tt2080374");
    await SeedMovieAsync(dbContext, "Molly's Game", "tt4669788");
    await SeedMovieAsync(dbContext, "The Trial of the Chicago 7", "tt1070874");
    await SeedMovieAsync(dbContext, "Being the Ricardos", "tt6009292");

    // ── Scenario 4: Martin Scorsese Super Draft ───────────────────────────────
    await SeedMovieAsync(dbContext, "Who's That Knocking at My Door", "tt0063876");
    await SeedMovieAsync(dbContext, "Boxcar Bertha", "tt0068232");
    await SeedMovieAsync(dbContext, "Mean Streets", "tt0070379");
    await SeedMovieAsync(dbContext, "Alice Doesn't Live Here Anymore", "tt0071115");
    await SeedMovieAsync(dbContext, "Taxi Driver", "tt0075314");
    await SeedMovieAsync(dbContext, "New York, New York", "tt0076451");
    await SeedMovieAsync(dbContext, "The Last Waltz", "tt0077838");
    await SeedMovieAsync(dbContext, "Raging Bull", "tt0081398");             // shared with Scenario 6
    await SeedMovieAsync(dbContext, "The King of Comedy", "tt0085794");
    await SeedMovieAsync(dbContext, "After Hours", "tt0088680");
    await SeedMovieAsync(dbContext, "The Color of Money", "tt0090863");      // shared with Scenario 6
    await SeedMovieAsync(dbContext, "The Last Temptation of Christ", "tt0095497");
    await SeedMovieAsync(dbContext, "Goodfellas", "tt0099685");
    await SeedMovieAsync(dbContext, "Cape Fear", "tt0101540");
    await SeedMovieAsync(dbContext, "The Age of Innocence", "tt0106226");
    await SeedMovieAsync(dbContext, "Casino", "tt0112641");
    await SeedMovieAsync(dbContext, "Kundun", "tt0119485");
    await SeedMovieAsync(dbContext, "Bringing Out the Dead", "tt0163988");
    await SeedMovieAsync(dbContext, "Gangs of New York", "tt0217505");
    await SeedMovieAsync(dbContext, "The Aviator", "tt0338751");
    await SeedMovieAsync(dbContext, "The Departed", "tt0407887");
    await SeedMovieAsync(dbContext, "Shutter Island", "tt1130884");
    await SeedMovieAsync(dbContext, "Hugo", "tt0970179");
    await SeedMovieAsync(dbContext, "The Wolf of Wall Street", "tt0993846");
    await SeedMovieAsync(dbContext, "Silence", "tt0490215");
    await SeedMovieAsync(dbContext, "The Irishman", "tt1302006");
    await SeedMovieAsync(dbContext, "No Direction Home", "tt0367631");
    await SeedMovieAsync(dbContext, "Killers of the Flower Moon", "tt5537002");
    await SeedMovieAsync(dbContext, "Shine a Light", "tt0893382");
    await SeedMovieAsync(dbContext, "Rolling Thunder Revue", "tt10293552");

    // ── Scenario 5: 2025 Mega Draft ───────────────────────────────────────────
    await SeedMovieAsync(dbContext, "Wake Up Dead Man: A Knives Out Mystery");
    await SeedMovieAsync(dbContext, "The Baltimorans");
    await SeedMovieAsync(dbContext, "Splitsville");
    await SeedMovieAsync(dbContext, "The Ballad of Wallis Island");
    await SeedMovieAsync(dbContext, "Lurker");
    await SeedMovieAsync(dbContext, "The Testament of Ann Lee");
    await SeedMovieAsync(dbContext, "Superman");
    await SeedMovieAsync(dbContext, "Sorry, Baby");
    await SeedMovieAsync(dbContext, "If I Had Legs I'd Kick You");
    await SeedMovieAsync(dbContext, "Twinless");
    await SeedMovieAsync(dbContext, "No Other Choice");
    await SeedMovieAsync(dbContext, "Cover-Up");
    await SeedMovieAsync(dbContext, "Hamnet");
    await SeedMovieAsync(dbContext, "Sirāt");
    await SeedMovieAsync(dbContext, "Blue Moon");
    await SeedMovieAsync(dbContext, "Sentimental Value");
    await SeedMovieAsync(dbContext, "Train Dreams");
    await SeedMovieAsync(dbContext, "Bugonia");
    await SeedMovieAsync(dbContext, "The Secret Agent");
    await SeedMovieAsync(dbContext, "Sinners");
    await SeedMovieAsync(dbContext, "One Battle After Another");
    // Guard test: 2024 film (wrong year for Scenario 5)
    await SeedMovieAsync(dbContext, "Wrong Year Film 2024", key: "WrongYear2024");

    // ── Scenario 6: 80's Sports Mini-Mega ────────────────────────────────────
    await SeedMovieAsync(dbContext, "Personal Best", "tt0084534");
    // The Color of Money (tt0090863) already seeded above
    await SeedMovieAsync(dbContext, "Major League", "tt0097815");
    await SeedMovieAsync(dbContext, "Hoosiers", "tt0091217");
    await SeedMovieAsync(dbContext, "Lucas", "tt0091501");
    await SeedMovieAsync(dbContext, "Eight Men Out", "tt0095016");
    await SeedMovieAsync(dbContext, "The Karate Kid", "tt0087538");
    await SeedMovieAsync(dbContext, "Field of Dreams", "tt0097351");
    // Raging Bull (tt0081398) already seeded above
    await SeedMovieAsync(dbContext, "Rocky III", "tt0084602");
    await SeedMovieAsync(dbContext, "The Natural", "tt0087781");
    await SeedMovieAsync(dbContext, "Bull Durham", "tt0094812");

    // ── Scenario 7: Spielberg Produced Mega Draft ─────────────────────────────
    await SeedMovieAsync(dbContext, "Real Steel", key: "Real Steel");
    await SeedMovieAsync(dbContext, "Jurassic Park III", key: "Jurassic Park III");
    await SeedMovieAsync(dbContext, "The Goonies", key: "The Goonies");
    await SeedMovieAsync(dbContext, "The Money Pit", key: "The Money Pit");
    await SeedMovieAsync(dbContext, "Innerspace", key: "Innerspace");
    await SeedMovieAsync(dbContext, "Arachnophobia", key: "Arachnophobia");
    await SeedMovieAsync(dbContext, "Joe Versus the Volcano", key: "Joe Versus the Volcano");
    await SeedMovieAsync(dbContext, "Back to the Future Part II", key: "Back to the Future Part II");
    await SeedMovieAsync(dbContext, "Transformers", key: "Transformers");
    await SeedMovieAsync(dbContext, "Men in Black 3", key: "Men in Black 3");
    await SeedMovieAsync(dbContext, "An American Tail: Fievel Goes West", key: "An American Tail: Fievel Goes West");
    await SeedMovieAsync(dbContext, "Letters from Iwo Jima", key: "Letters from Iwo Jima");
    await SeedMovieAsync(dbContext, "Gremlins 2: The New Batch", key: "Gremlins 2: The New Batch");
    await SeedMovieAsync(dbContext, "Poltergeist", key: "Poltergeist");
    await SeedMovieAsync(dbContext, "True Grit", key: "True Grit");
    await SeedMovieAsync(dbContext, "Deep Impact", key: "Deep Impact");
    await SeedMovieAsync(dbContext, "First Man", key: "First Man");
    await SeedMovieAsync(dbContext, "Men in Black", key: "Men in Black");
    await SeedMovieAsync(dbContext, "Twister", key: "Twister");
    await SeedMovieAsync(dbContext, "Who Framed Roger Rabbit", key: "Who Framed Roger Rabbit");
    await SeedMovieAsync(dbContext, "Back to the Future", key: "Back to the Future");

    await dbContext.SaveChangesAsync();
    _seeded = true;
  }

  private async Task SeedMovieAsync(
    DraftsDbContext dbContext,
    string title,
    string? imdbId = null,
    string? key = null)
  {
    // Deduplicate by IMDb ID when provided
    var lookupKey = imdbId ?? key ?? title;
    if (_publicIdByKey.ContainsKey(lookupKey)) return;

    var existing = imdbId is not null
      ? await dbContext.Movies.FirstOrDefaultAsync(m => m.ImdbId == imdbId)
      : null;

    if (existing is not null)
    {
      _publicIdByKey[lookupKey] = existing.PublicId;
      // Also register by title for convenience
      _publicIdByKey.TryAdd(title, existing.PublicId);
      return;
    }

    var publicId = $"m_{Guid.NewGuid():N}";
    var movie = Movie.Create(
      movieTitle: title,
      publicId: publicId,
      mediaType: MediaType.Movie,
      id: Guid.NewGuid(),
      imdbId: imdbId).Value;

    dbContext.Movies.Add(movie);
    _publicIdByKey[lookupKey] = publicId;
    _publicIdByKey.TryAdd(title, publicId);
  }
}
