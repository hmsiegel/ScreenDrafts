namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Helpers;

public sealed record DraftSeed(Guid Id, string PublicId, string Title);

public sealed record PartSeed(Guid Id, DraftSeed Draft, int Index);

public sealed record PersonSeed(Guid Id, string PublicId, string DisplayName);

public sealed record DrafterSeed(Guid Id, string PublicId, PersonSeed Person);

public sealed record HostSeed(Guid Id, PersonSeed Person);

public sealed record ParticipantSeed(Guid RowId, Guid ParticipantId, int Kind);

public sealed record MovieSeed(Guid Id, string Title);

public sealed record SubDraftSeed(Guid Id, int Index);

public sealed record PickSeed(
  Guid Id,
  PartSeed Part,
  ParticipantSeed Participant,
  MovieSeed Movie,
  int Position,
  int PlayOrder
);

/// <summary>
/// Seeds the <c>drafts</c> schema with parameterized SQL through the module's own
/// <see cref="DraftsDbContext"/>, so wiki-export tests can describe a scenario in a handful of
/// lines instead of driving the full command pipeline for every pick and veto. Column names come
/// from the EF model snapshot; only columns that are NOT NULL without a database default (plus the
/// ones a scenario cares about) are written.
/// </summary>
public sealed class WikiSeeder(DraftsDbContext db)
{
  public const int MainFeed = 0;
  public const int Patreon = 1;

  public const int DrafterKind = 0;
  public const int TeamKind = 1;
  public const int CommunityKind = 2;

  private static readonly Faker _faker = new();

  private readonly DraftsDbContext _db = db;
  private Guid? _seriesId;
  private int _nextTmdbId = 10_000;

  private static DateTime Now => DateTime.UtcNow;

  private static string PublicId(string prefix) => $"{prefix}_{_faker.Random.AlphaNumeric(15)}";

  private Task<int> ExecuteAsync(FormattableString sql) =>
    _db.Database.ExecuteSqlAsync(sql, TestContext.Current.CancellationToken);

  // ── Shared structure ────────────────────────────────────────────────────

  private async Task<Guid> SeriesIdAsync()
  {
    if (_seriesId is { } existing)
    {
      return existing;
    }

    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.series
        (id, public_id, name, kind, canonical_policy, continuity_scope, continuity_date_rule,
         allowed_draft_types, created_at_utc, is_deleted)
      VALUES
        ({id}, {PublicId("s")}, {"Wiki Test Series"}, 0, 0, 0, 0, 0, {Now}, false)
      """
    );

    _seriesId = id;
    return id;
  }

  public async Task<Guid> CampaignAsync(string name)
  {
    ArgumentNullException.ThrowIfNull(name);

    var id = Guid.NewGuid();
    var slug = $"{name.ToUpperInvariant().Replace(' ', '-')}-{_faker.Random.AlphaNumeric(6)}";

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.campaigns (id, public_id, slug, name, created_at_utc, is_deleted)
      VALUES ({id}, {PublicId("c")}, {slug}, {name}, {Now}, false)
      """
    );

    return id;
  }

  public async Task CategoryAsync(DraftSeed draft, string name, bool isDeleted = false)
  {
    ArgumentNullException.ThrowIfNull(draft);

    var categoryId = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.categories (id, public_id, name, created_on_utc, is_deleted)
      VALUES ({categoryId}, {PublicId("cat")}, {name}, {Now}, {isDeleted})
      """
    );

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_categories (draft_id, category_id)
      VALUES ({draft.Id}, {categoryId})
      """
    );
  }

  // ── People ──────────────────────────────────────────────────────────────

  public async Task<PersonSeed> PersonAsync(
    string displayName,
    string? firstName = null,
    string? lastName = null,
    string? twitter = null,
    string? letterboxd = null,
    string? instagram = null,
    string? bluesky = null
  )
  {
    ArgumentNullException.ThrowIfNull(displayName);

    var tokens = displayName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var first = firstName ?? tokens.FirstOrDefault() ?? displayName;
    var last = lastName ?? (tokens.Length > 1 ? tokens[1] : string.Empty);
    var id = Guid.NewGuid();
    var publicId = PublicId("pe");

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.people
        (id, public_id, first_name, last_name, display_name,
         twitter_handle, letterboxd_handle, instagram_handle, bluesky_handle)
      VALUES
        ({id}, {publicId}, {first}, {last}, {displayName},
         {twitter}, {letterboxd}, {instagram}, {bluesky})
      """
    );

    return new PersonSeed(id, publicId, displayName);
  }

  public async Task<DrafterSeed> DrafterAsync(
    string displayName,
    string? firstName = null,
    string? lastName = null,
    string? twitter = null,
    string? letterboxd = null,
    string? instagram = null,
    string? bluesky = null
  )
  {
    var person = await PersonAsync(
      displayName,
      firstName,
      lastName,
      twitter,
      letterboxd,
      instagram,
      bluesky
    );
    var id = Guid.NewGuid();
    var publicId = PublicId("dr");

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.drafters (id, public_id, person_id, is_retired)
      VALUES ({id}, {publicId}, {person.Id}, false)
      """
    );

    return new DrafterSeed(id, publicId, person);
  }

  public async Task<HostSeed> HostAsync(string displayName) =>
    await HostForPersonAsync(await PersonAsync(displayName));

  /// <summary>Makes an existing person (for example a drafter's) a host as well.</summary>
  public async Task<HostSeed> HostForPersonAsync(PersonSeed person)
  {
    ArgumentNullException.ThrowIfNull(person);

    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.hosts (id, public_id, person_id, is_retired)
      VALUES ({id}, {PublicId("h")}, {person.Id}, false)
      """
    );

    return new HostSeed(id, person);
  }

  // ── Drafts, parts, releases ─────────────────────────────────────────────

  /// <summary>
  /// Creates a draft. A main-feed episode number, when given, is stored on the main-feed channel
  /// release; use <see cref="ChannelReleaseAsync"/> for any other channel.
  /// </summary>
  public async Task<DraftSeed> DraftAsync(
    string title,
    Guid? campaignId = null,
    int? episodeNumber = null
  )
  {
    var id = Guid.NewGuid();
    var publicId = PublicId("d");
    var seriesId = await SeriesIdAsync();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.drafts
        (id, public_id, title, created_at_utc, series_id, draft_type, draft_status,
         is_hostless, campaign_id, is_deleted)
      VALUES
        ({id}, {publicId}, {title}, {Now}, {seriesId}, 0, 0, false, {campaignId}, false)
      """
    );

    var draft = new DraftSeed(id, publicId, title);

    if (episodeNumber is not null)
    {
      await ChannelReleaseAsync(draft, MainFeed, episodeNumber);
    }

    return draft;
  }

  public async Task ChannelReleaseAsync(DraftSeed draft, int channel, int? episodeNumber)
  {
    ArgumentNullException.ThrowIfNull(draft);

    var seriesId = await SeriesIdAsync();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_channel_releases
        (draft_id, release_channel, created_on_utc, episode_number, series_id)
      VALUES ({draft.Id}, {channel}, {Now}, {episodeNumber}, {seriesId})
      """
    );
  }

  public async Task<PartSeed> PartAsync(
    DraftSeed draft,
    int index,
    DateOnly? mainFeed = null,
    DateOnly? patreon = null
  )
  {
    ArgumentNullException.ThrowIfNull(draft);

    var id = Guid.NewGuid();
    var seriesId = await SeriesIdAsync();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_parts
        (id, created_at_utc, draft_id, draft_public_id, draft_type, is_hostless,
         movie_version_policy_type, part_index, public_id, series_id, status)
      VALUES
        ({id}, {Now}, {draft.Id}, {draft.PublicId}, 0, false, 0, {index}, {PublicId("dp")},
         {seriesId}, 0)
      """
    );

    var part = new PartSeed(id, draft, index);

    if (mainFeed is { } main)
    {
      await ReleaseAsync(part, MainFeed, main);
    }

    if (patreon is { } patreonDate)
    {
      await ReleaseAsync(part, Patreon, patreonDate);
    }

    return part;
  }

  public Task<int> ReleaseAsync(PartSeed part, int channel, DateOnly date)
  {
    ArgumentNullException.ThrowIfNull(part);

    return ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_releases (part_id, release_channel, created_on_utc, release_date)
      VALUES ({part.Id}, {channel}, {Now}, {date})
      """
    );
  }

  public Task<int> HostOnPartAsync(PartSeed part, HostSeed host, int role)
  {
    ArgumentNullException.ThrowIfNull(part);
    ArgumentNullException.ThrowIfNull(host);

    return ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_hosts (draft_part_id, host_id, role)
      VALUES ({part.Id}, {host.Id}, {role})
      """
    );
  }

  // ── Participants ────────────────────────────────────────────────────────

  public async Task<ParticipantSeed> ParticipantAsync(
    PartSeed part,
    Guid participantId,
    int kind = DrafterKind,
    int vetoesUsed = 0,
    int startingVetoes = 0,
    int vetoesRollingIn = 0,
    int awardedVetoes = 0,
    int vetoOverridesRollingIn = 0,
    int awardedVetoOverrides = 0,
    int vetoOverridesUsed = 0
  )
  {
    ArgumentNullException.ThrowIfNull(part);

    var rowId = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_part_participants
        (id, draft_part_id, participant_id_value, participant_kind_value,
         awarded_fungible_tokens, awarded_veto_overrides, awarded_vetoes, commissioner_overrides,
         fungible_tokens, fungible_tokens_rolling_in, fungible_tokens_used, starting_vetoes,
         veto_overrides_rolling_in, veto_overrides_used, vetoes_rolling_in, vetoes_used)
      VALUES
        ({rowId}, {part.Id}, {participantId}, {kind},
         0, {awardedVetoOverrides}, {awardedVetoes}, 0,
         0, 0, 0, {startingVetoes},
         {vetoOverridesRollingIn}, {vetoOverridesUsed}, {vetoesRollingIn}, {vetoesUsed})
      """
    );

    return new ParticipantSeed(rowId, participantId, kind);
  }

  public Task<ParticipantSeed> ParticipantAsync(
    PartSeed part,
    DrafterSeed drafter,
    int vetoesUsed = 0,
    int startingVetoes = 0,
    int vetoesRollingIn = 0,
    int awardedVetoes = 0,
    int vetoOverridesRollingIn = 0,
    int awardedVetoOverrides = 0,
    int vetoOverridesUsed = 0
  )
  {
    ArgumentNullException.ThrowIfNull(drafter);

    return ParticipantAsync(
      part,
      drafter.Id,
      DrafterKind,
      vetoesUsed,
      startingVetoes,
      vetoesRollingIn,
      awardedVetoes,
      vetoOverridesRollingIn,
      awardedVetoOverrides,
      vetoOverridesUsed
    );
  }

  public Task<ParticipantSeed> CommunityAsync(PartSeed part) =>
    ParticipantAsync(part, Guid.NewGuid(), CommunityKind);

  // ── Movies, picks, vetoes ───────────────────────────────────────────────

  public async Task<MovieSeed> MovieAsync(string title, string? year = null)
  {
    var id = Guid.NewGuid();
    var tmdbId = _nextTmdbId++;

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.movies (id, public_id, movie_title, media_type, year, tmdb_id)
      VALUES ({id}, {PublicId("m")}, {title}, 0, {year}, {tmdbId})
      """
    );

    return new MovieSeed(id, title);
  }

  public async Task<SubDraftSeed> SubDraftAsync(PartSeed part, int index)
  {
    ArgumentNullException.ThrowIfNull(part);

    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.sub_drafts (id, draft_part_id, "index", public_id, status)
      VALUES ({id}, {part.Id}, {index}, {PublicId("sd")}, 0)
      """
    );

    return new SubDraftSeed(id, index);
  }

  public async Task<PickSeed> PickAsync(
    PartSeed part,
    ParticipantSeed participant,
    MovieSeed movie,
    int position,
    int playOrder,
    SubDraftSeed? subDraft = null
  )
  {
    ArgumentNullException.ThrowIfNull(part);
    ArgumentNullException.ThrowIfNull(participant);
    ArgumentNullException.ThrowIfNull(movie);

    var id = Guid.NewGuid();
    Guid? subDraftId = subDraft?.Id;

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.picks
        (id, draft_part_id, movie_id, play_order, played_by_participant_id,
         played_by_participant_id_value, played_by_participant_kind_value, position, sub_draft_id)
      VALUES
        ({id}, {part.Id}, {movie.Id}, {playOrder}, {participant.RowId},
         {participant.ParticipantId}, {participant.Kind}, {position}, {subDraftId})
      """
    );

    return new PickSeed(id, part, participant, movie, position, playOrder);
  }

  /// <summary>
  /// Records a veto on a pick. Passing <paramref name="overriddenBy"/> also records the override
  /// and marks the veto overridden.
  /// </summary>
  public async Task VetoAsync(
    PickSeed pick,
    ParticipantSeed issuedBy,
    int sequence,
    ParticipantSeed? overriddenBy = null
  )
  {
    ArgumentNullException.ThrowIfNull(pick);
    ArgumentNullException.ThrowIfNull(issuedBy);

    var vetoId = Guid.NewGuid();
    var isOverridden = overriddenBy is not null;

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.vetoes
        (id, is_overridden, issued_by_participant_id, occurred_on, sequence,
         spent_from_fungible_pool, target_pick_id)
      VALUES
        ({vetoId}, {isOverridden}, {issuedBy.RowId}, {Now}, {sequence}, false, {pick.Id})
      """
    );

    if (overriddenBy is not null)
    {
      await ExecuteAsync(
        $"""
        INSERT INTO drafts.veto_overrides
          (id, issued_by_participant_id, spent_from_fungible_pool, veto_id)
        VALUES ({Guid.NewGuid()}, {overriddenBy.RowId}, false, {vetoId})
        """
      );
    }
  }

  public Task<int> CommissionerOverrideAsync(PickSeed pick)
  {
    ArgumentNullException.ThrowIfNull(pick);

    return ExecuteAsync(
      $"""
      INSERT INTO drafts.commissioner_overrides (id, pick_id)
      VALUES ({Guid.NewGuid()}, {pick.Id})
      """
    );
  }

  public Task<int> TriviaAsync(
    PartSeed part,
    int position,
    int questionsWon,
    Guid participantId,
    int participantKind = DrafterKind
  )
  {
    ArgumentNullException.ThrowIfNull(part);

    return ExecuteAsync(
      $"""
      INSERT INTO drafts.trivia_results
        (id, draft_part_id, position, questions_won, participant_kind, participant_id)
      VALUES
        ({Guid.NewGuid()}, {part.Id}, {position}, {questionsWon}, {participantKind}, {participantId})
      """
    );
  }

  // ── Predictions ─────────────────────────────────────────────────────────

  public async Task<Guid> PredictionSeasonAsync(int number, DateOnly startsOn)
  {
    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.prediction_seasons (id, number, public_id, starts_on)
      VALUES ({id}, {number}, {PublicId("ps")}, {startsOn})
      """
    );

    return id;
  }

  public async Task<Guid> ContestantAsync(string displayName)
  {
    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.prediction_contestants (id, display_name, is_active, public_id)
      VALUES ({id}, {displayName}, true, {PublicId("pc")})
      """
    );

    return id;
  }

  public Task<int> PredictionRuleAsync(PartSeed part, int mode)
  {
    ArgumentNullException.ThrowIfNull(part);

    return ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_part_prediction_rules
        (id, created_on_utc, draft_part_id, prediction_mode, public_id)
      VALUES ({Guid.NewGuid()}, {Now}, {part.Id}, {mode}, {PublicId("pr")})
      """
    );
  }

  public async Task<Guid> PredictionSetAsync(
    PartSeed part,
    Guid contestantId,
    Guid seasonId,
    DateTime submittedAtUtc
  )
  {
    ArgumentNullException.ThrowIfNull(part);

    var id = Guid.NewGuid();

    await ExecuteAsync(
      $"""
      INSERT INTO drafts.draft_prediction_sets
        (id, contestant_id, draft_part_id, public_id, season_id, source_kind, submitted_at_utc)
      VALUES
        ({id}, {contestantId}, {part.Id}, {PublicId("set")}, {seasonId}, 0, {submittedAtUtc})
      """
    );

    return id;
  }

  public Task<int> PredictionEntryAsync(
    Guid setId,
    string title,
    bool? isCorrect,
    int? orderIndex = null
  ) =>
    ExecuteAsync(
      $"""
      INSERT INTO drafts.prediction_entries
        (id, is_correct, media_title, order_index, set_id, tmdb_id)
      VALUES
        ({Guid.NewGuid()}, {isCorrect}, {title}, {orderIndex}, {setId}, {_nextTmdbId++})
      """
    );

  public Task<int> PredictionResultAsync(Guid setId, int pointsAwarded) =>
    ExecuteAsync(
      $"""
      INSERT INTO drafts.prediction_results
        (id, correct_count, points_awarded, scored_at_utc, set_id)
      VALUES ({Guid.NewGuid()}, 0, {pointsAwarded}, {Now}, {setId})
      """
    );

  public Task<int> CarryoverAsync(Guid contestantId, Guid seasonId, int points) =>
    ExecuteAsync(
      $"""
      INSERT INTO drafts.prediction_carryovers (id, contestant_id, kind, points, season_id)
      VALUES ({Guid.NewGuid()}, {contestantId}, 0, {points}, {seasonId})
      """
    );
}
