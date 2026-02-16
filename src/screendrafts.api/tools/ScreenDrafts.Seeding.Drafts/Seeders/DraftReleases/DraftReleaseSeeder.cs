using ScreenDrafts.Modules.Drafts.Domain.Drafts.Entities;
using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate;

namespace ScreenDrafts.Seeding.Drafts.Seeders.DraftReleases;

internal sealed partial class DraftReleaseSeeder(
  DraftsDbContext dbContext,
  ILogger<DraftReleaseSeeder> logger,
  ICsvFileService csvFileService)
  : DraftBaseSeeder(
    dbContext,
    logger,
    csvFileService), ICustomSeeder
{
  public int Order => 6;
  public string Name => "draft_releases";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedDraftReleasesAsync(cancellationToken);

  private async Task SeedDraftReleasesAsync(CancellationToken cancellationToken)
  {
    const string TableName = "DraftReleases";

    var rows = ReadCsv<DraftReleaseCsvModel>(
      new SeedFile(FileNames.DraftReleasesSeeder, SeedFileType.Csv),
      TableName);

    if (rows is null || rows.Count == 0)
    {
      return;
    }

    var partIds = rows
      .Select(r => DraftPartId.Create(r.PartId))
      .Distinct()
      .ToList();

    var partToDraftRows = await _dbContext.DraftParts
      .AsNoTracking()
      .Where(p => partIds.Contains(p.Id))
      .Select(p => new
      {
        PartId = p.Id,
        p.DraftId
      })
      .ToListAsync(cancellationToken);

    var draftIdByPartId = partToDraftRows.ToDictionary(x => x.PartId.Value, x => x.DraftId);

    foreach (var r in rows.Where(r => !draftIdByPartId.ContainsKey(r.PartId)))
    {
      Log_DraftPartNotFound(Format(r));
    }

    var resolved = rows
      .Where(r => draftIdByPartId.ContainsKey(r.PartId))
      .Select(r => new ResolvedRow(
        DraftPartId: DraftPartId.Create(r.PartId),
        DraftId: draftIdByPartId[r.PartId],
        ReleaseChannel: ReleaseChannel.FromValue(r.ReleaseChannelId),
        EpisodeNumber: r.EpisodeNumber,
        ReleaseDate: DateOnly.Parse(r.ReleaseDate, CultureInfo.InvariantCulture),
        SeriesId: SeriesId.Create(r.SeriesId)))
      .ToList();


    var existingPartReleases = await _dbContext.DraftReleases
      .Where(dr => partIds.Contains(dr.PartId))
      .ToListAsync(cancellationToken);

    var existingByKey = existingPartReleases.ToDictionary(
      x => (x.PartId.Value, x.ReleaseChannel.Value),
      x => x);

    int inserts = 0, updates = 0;

    foreach (var r in resolved)
    {
      var key = (r.DraftPartId.Value, r.ReleaseChannel.Value);

      if (!existingByKey.TryGetValue(key, out var entity))
      {
        var create = DraftRelease.Create(
          partId: r.DraftPartId,
          releaseChannel: r.ReleaseChannel,
          releaseDate: r.ReleaseDate);

        if (create.IsFailure)
        {
          Log_CreateDraftReleaseFailed(r.DraftPartId.Value, r.ReleaseChannel.Value, string.Join("; ", create.Errors.Select(e => e.Code)));
          continue;
        }

        _dbContext.DraftReleases.Add(create.Value);
        inserts++;
      }
      else
      {
        if (entity.ReleaseDate != r.ReleaseDate)
        {
          var set = entity.SetReleaseDate(r.ReleaseDate);

          if (set.IsFailure)
          {
            Log_UpdateReleaseDateFailed(entity.PartId.Value, entity.ReleaseChannel.Value, string.Join("; ", set.Errors.Select(e => e.Code)));
            continue;
          }

          updates++;
        }
      }
    }

    var episodeByDraftChannel = new Dictionary<(Guid DraftId, int ReleaseChannel, Guid SeriesId), int?>();

    foreach (var g in resolved.GroupBy(x => (x.DraftId.Value, x.ReleaseChannel.Value, x.SeriesId.Value)))
    {
      var nonNull = g.Select(x => x.EpisodeNumber).Where(en => en.HasValue).Select(en => en!.Value).Distinct().ToList();

      if (nonNull.Count == 0)
      {
        episodeByDraftChannel[(g.Key.Item1, g.Key.Item2, g.Key.Item3)] = null;
        continue;
      }

      if (nonNull.Count > 1)
      {
        Log_InconsistentEpisodeNumbers(g.Key.Item1, g.Key.Item2, string.Join(", ", nonNull));

        episodeByDraftChannel[(g.Key.Item1, g.Key.Item2, g.Key.Item3)] = null;
        continue;
      }

      episodeByDraftChannel[(g.Key.Item1, g.Key.Item2, g.Key.Item3)] = nonNull.Single();
    }

    var candidates = episodeByDraftChannel
      .Where(kv => kv.Value.HasValue)
      .Select(kv => new
      {
        kv.Key.DraftId,
        kv.Key.ReleaseChannel,
        EpisodeNumber = kv.Value!.Value,
        kv.Key.SeriesId
      })
      .ToList();

    var collisions = candidates
      .GroupBy(x => (x.ReleaseChannel, x.EpisodeNumber, x.SeriesId))
      .Where(g => g.Select(x => x.DraftId).Distinct().Count() > 1)
      .ToList();

    foreach (var c in collisions)
    {
      Log_EpisodeNumberCollision(c.Key.ReleaseChannel, c.Key.EpisodeNumber, string.Join(", ", c.Select(x => x.DraftId)));
    }

    var collisionKeys = collisions.Select(c => c.Key).ToHashSet();

    var validCandidates = candidates
      .Where(c => !collisionKeys.Contains((c.ReleaseChannel, c.EpisodeNumber, c.SeriesId)))
      .ToList();

    var draftIds = validCandidates.Select(c => DraftId.Create(c.DraftId)).Distinct().ToList();

    var existingChannelReleases = await _dbContext.DraftChannelReleases
      .Where(dcr => draftIds.Contains(dcr.DraftId))
      .ToListAsync(cancellationToken);

    var existingChannelByKey = existingChannelReleases.ToDictionary(
      x => (x.DraftId.Value, x.ReleaseChannel.Value, x.SeriesId.Value),
      x => x);

    int channelInserts = 0, channelUpdates = 0;

    foreach (var c in validCandidates)
    {
      var key = (c.DraftId, c.ReleaseChannel, c.SeriesId);

      if (!existingChannelByKey.TryGetValue(key, out var entity))
      {
        var create = DraftChannelRelease.Create(
          draftId: DraftId.Create(c.DraftId),
          releaseChannel: ReleaseChannel.FromValue(c.ReleaseChannel),
          seriesId: SeriesId.Create(c.SeriesId),
          episodeNumber: c.EpisodeNumber);

        if (create.IsFailure)
        {
          Log_CreateDraftChannelReleaseFailed(c.DraftId, c.ReleaseChannel, c.EpisodeNumber, string.Join("; ", create.Errors.Select(e => e.Code)));
          continue;
        }

        _dbContext.DraftChannelReleases.Add(create.Value);
        channelInserts++;
      }
      else
      {
        if (entity.EpisodeNumber != c.EpisodeNumber)
        {
          var set = entity.SetEpisodeNumber(c.EpisodeNumber);
          if (set.IsFailure)
          {
            Log_UpdateEpisodeNumberFailed(c.DraftId, c.ReleaseChannel, string.Join("; ", set.Errors.Select(e => e.Code)));
            continue;
          }
          channelUpdates++;
        }
      }
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
    Log_SeedingCompleted(inserts, updates, rows.Count - inserts - updates);
  }

  private static string Format(DraftReleaseCsvModel model)
    => $"DraftPartId: {model.PartId}, ReleaseChannelId: {model.ReleaseChannelId}, EpisodeNumber: {model.EpisodeNumber}, ReleaseDate: {model.ReleaseDate}";

  private sealed record ResolvedRow(
    DraftPartId DraftPartId,
    DraftId DraftId,
    SeriesId SeriesId,
    ReleaseChannel ReleaseChannel,
    int? EpisodeNumber,
    DateOnly ReleaseDate);

  // LoggerMessage delegates
  [LoggerMessage(LogLevel.Error, "DraftPart not found for DraftRelease record: {Record}")]
  private partial void Log_DraftPartNotFound(string record);

  [LoggerMessage(LogLevel.Error, "Inconsistent episode numbers for DraftId: {DraftId}, ReleaseChannel: {ReleaseChannel}. Distinct episode numbers: {EpisodeNumbers}")]
  private partial void Log_InconsistentEpisodeNumbers(Guid draftId, int releaseChannel, string episodeNumbers);

  [LoggerMessage(LogLevel.Error, "Episode number collision for ReleaseChannel: {ReleaseChannel}, EpisodeNumber: {EpisodeNumber}. Colliding DraftIds: {DraftIds}")]
  private partial void Log_EpisodeNumberCollision(int releaseChannel, int episodeNumber, string draftIds);

  [LoggerMessage(LogLevel.Error, "Failed to create DraftRelease entity (DraftPartId: {DraftPartId}, ReleaseChannel: {ReleaseChannel}). Errors: {Errors}")]
  private partial void Log_CreateDraftReleaseFailed(Guid draftPartId, int releaseChannel, string errors);

  [LoggerMessage(LogLevel.Error, "Failed to create DraftChannelRelease entity (DraftId: {DraftId}, ReleaseChannel: {ReleaseChannel}, EpisodeNumber: {EpisodeNumber}). Errors: {Errors}")]
  private partial void Log_CreateDraftChannelReleaseFailed(Guid draftId, int releaseChannel, int? episodeNumber, string errors);

  [LoggerMessage(LogLevel.Error, "Failed to update ReleaseDate for DraftRelease entity (PartId: {PartId}, ReleaseChannel: {ReleaseChannel}). Errors: {Errors}")]
  private partial void Log_UpdateReleaseDateFailed(Guid partId, int releaseChannel, string errors);

  [LoggerMessage(LogLevel.Error, "DraftId mismatch for existing DraftRelease entity. Record: {Record}, Existing DraftId: {ExistingDraftId}, New DraftId: {NewDraftId}")]
  private partial void Log_DraftIdMismatch(string record, Guid existingDraftId, Guid newDraftId);

  [LoggerMessage(LogLevel.Error, "Failed to update EpisodeNumber for DraftChannelRelease entity (DraftId: {DraftId}, ReleaseChannel: {ReleaseChannel}). Errors: {Errors}")]
  private partial void Log_UpdateEpisodeNumberFailed(Guid draftId, int releaseChannel, string errors);

  [LoggerMessage(LogLevel.Information, "DraftRelease seeding completed. Inserts: {Inserts}, Updates: {Updates}, Skipped: {Skipped}")]
  private partial void Log_SeedingCompleted(int inserts, int updates, int skipped);
}
