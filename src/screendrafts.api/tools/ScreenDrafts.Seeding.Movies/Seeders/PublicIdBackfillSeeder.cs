namespace ScreenDrafts.Seeding.Movies.Seeders;

internal sealed partial class PublicIdBackfillSeeder(
  MoviesDbContext dbContext,
  IPublicIdGenerator publicIdGenerator,
  ILogger<PublicIdBackfillSeeder> logger,
  IDbConnectionFactory dbConnectionFactory)
  : ICustomSeeder
{
  private readonly MoviesDbContext _dbContext = dbContext;
  private IPublicIdGenerator _publicIdGenerator = publicIdGenerator;
  private readonly ILogger<PublicIdBackfillSeeder> _logger = logger;
  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;

  public int Order => 1;
  public string Name => "publicidbackfill";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await BackfillMoviesModuleAsync(cancellationToken);
    await BackfillDraftsModuleAsync(cancellationToken);
  }

  private async Task BackfillMoviesModuleAsync(CancellationToken cancellationToken)
  {
    var allRows = await _dbContext.Media
      .Where(m => m.PublicId.Length == 17)
      .ToListAsync(cancellationToken);

    var rows = allRows
      .Where(m => IsPlaceholder(m.PublicId))
      .ToList();

    if (rows.Count == 0)
    {
      Log_NothingToBackfill(_logger);
      return;
    }

    Log_StartingBackfill(_logger, rows.Count);


    foreach (var media in rows)
    {
      var newPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Media);
      media.SetPublicId(newPublicId);
    }

    await _dbContext.SaveChangesAsync(cancellationToken);
    Log_MoviesModuleUpdated(_logger, rows.Count);

  }

  private async Task BackfillDraftsModuleAsync(CancellationToken cancellationToken)
  {
    await using var connection = await _dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var draftsPlaceholders = (await connection.QueryAsync<DraftsPlaceholderRow>("""
      SELECT id AS MediaId, public_id as PublicId
      FROM drafts.movies
      WHERE LENGTH(public_id) = 17
      """)).Where(r => IsPlaceholder(r.PublicId)).ToList();

    if (draftsPlaceholders.Count == 0)
    {
      Log_NothingToBackfill(_logger);
      return;
    }

    Log_StartingBackfill(_logger, draftsPlaceholders.Count);
    var draftIds = draftsPlaceholders.Select(r => r.MediaId).ToArray();

    var realPublicIds = (await connection.QueryAsync<MediaPublicIdRow>("""
      SELECT id AS MediaId, public_id as PublicId
      FROM movies.media
      WHERE id = ANY(@DraftIds)
      """, new { DraftIds = draftIds })).ToDictionary(r => r.MediaId, r => r.PublicId);

    var idMap = draftsPlaceholders
      .Where(dp => realPublicIds.ContainsKey(dp.MediaId))
      .Select(r => new PublicIdMapping(r.MediaId, realPublicIds[r.MediaId]))
      .ToList();

    var unmatched = draftsPlaceholders.Count - idMap.Count;
    if (unmatched > 0)
    {
      Log_DraftsUnmatched(_logger, unmatched);
    }

    if (idMap.Count == 0)
    {
      Log_NothingToBackfill(_logger);
      return;
    }

    foreach (var batch in idMap.Chunk(500))
    {
      await connection.ExecuteAsync("""
        CREATE TEMP TABLE IF NOT EXISTS _public_id_map (
          media_id UUID NOT NULL,
          new_public_id TEXT NOT NULL
        ) ON COMMIT DELETE ROWS;
        """);

      await connection.ExecuteAsync("""
        INSERT INTO _public_id_map (media_id, new_public_id) VALUES (@MediaId, @NewPublicId)
        """, batch);

      var affected = await connection.ExecuteAsync("""
        UPDATE drafts.movies AS dm
        SET public_id = m.new_public_id
        FROM _public_id_map AS m
        WHERE dm.id = m.media_id
        """);

      await connection.ExecuteAsync("DELETE FROM _public_id_map;");

      Log_DraftsModuleUpdated(_logger, affected);
    }

  }
  private static bool IsPlaceholder(string publicId)
  {
    if (publicId.Length != 17 || !publicId.StartsWith("m_", StringComparison.OrdinalIgnoreCase))
    {
      return false;
    }

    var suffix = publicId.AsSpan(2);
    foreach (var c in suffix)
    {
      if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f')))
      {
        return false;
      }
    }

    return true;
  }

  private sealed record PublicIdMapping(Guid MediaId, string NewPublicId);
  private sealed record DraftsPlaceholderRow(Guid MediaId, string PublicId);
  private sealed record MediaPublicIdRow(Guid MediaId, string PublicId);

  [LoggerMessage(Level = LogLevel.Information,
  Message = "No placeholder public_ids found. Nothing to backfill.")]
  private static partial void Log_NothingToBackfill(ILogger<PublicIdBackfillSeeder> logger);

  [LoggerMessage(Level = LogLevel.Information,
    Message = "Starting PublicId backfill for {count} media rows.")]
  private static partial void Log_StartingBackfill(ILogger<PublicIdBackfillSeeder> logger, int count);
  [LoggerMessage(Level = LogLevel.Information,
    Message = "Movies module: updated {count} public_ids.")]
  private static partial void Log_MoviesModuleUpdated(ILogger<PublicIdBackfillSeeder> logger, int count);
  [LoggerMessage(Level = LogLevel.Information,
    Message = "Drafts module: updated {count} matching rows.")]
  private static partial void Log_DraftsModuleUpdated(ILogger<PublicIdBackfillSeeder> logger, int count);

  [LoggerMessage(Level = LogLevel.Information,
    Message = "PublicId backfill complete. {count} records updated across both modules.")]
  private static partial void Log_Complete(ILogger<PublicIdBackfillSeeder> logger, int count);

  [LoggerMessage(Level = LogLevel.Warning,
    Message = "{count} drafts rows with placeholder public_ids did not match any media rows.")]
  private static partial void Log_DraftsUnmatched(ILogger<PublicIdBackfillSeeder> logger, int count);
}
