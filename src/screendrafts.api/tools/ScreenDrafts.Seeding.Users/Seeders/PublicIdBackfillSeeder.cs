using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace ScreenDrafts.Seeding.Users.Seeders;

internal sealed class PublicIdBackfillSeeder(
  ILogger<PublicIdBackfillSeeder> logger,
  ICsvFileService csvFileService,
  UsersDbContext dbContext,
  IPublicIdGenerator publicIdGenerator)
  : UserBaseSeeder(
    logger,
    csvFileService,
    dbContext), ICustomSeeder
{
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 3;
  public string Name => "publicidbackfill";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => BackfillPublicIdsAsync(cancellationToken);

  private static readonly Action<ILogger, object?, Exception?> _logSamplePublicIds =
    LoggerMessage.Define<object?>(
      LogLevel.Information,
      new EventId(0, nameof(BackfillPublicIdsAsync)),
      "Sample PublicIds: {Sample}");

  private static readonly Action<ILogger, int, Exception?> _logUsersWithoutPublicId =
    LoggerMessage.Define<int>(
      LogLevel.Information,
      new EventId(1, nameof(BackfillPublicIdsAsync)),
      "Users without PublicId: {Count}");

  private static readonly Action<ILogger, int, int, Exception?> _logBackfilledUsers =
    LoggerMessage.Define<int, int>(
      LogLevel.Information,
      new EventId(2, nameof(BackfillPublicIdsAsync)),
      "Backfilled {UsersCount} users (SaveChanges wrote {Saved} rows).");

  private async Task BackfillPublicIdsAsync(CancellationToken cancellationToken)
  {
    var sample = await _dbContext.Users
        .OrderBy(u => u.Id)
    .Select(u => new { u.Id, u.PublicId})
    .Take(5)
    .ToListAsync(cancellationToken);

    _logSamplePublicIds(_logger, sample, null);

    var usersWithoutPublicId = await _dbContext.Users
        .Where(u => u.PublicId == null || u.PublicId == "")
        .ToListAsync(cancellationToken);

    _logUsersWithoutPublicId(_logger, usersWithoutPublicId.Count, null);

    if (usersWithoutPublicId.Count == 0)
    {
      return;
    }

    var existing = await _dbContext.Users
      .Where(u => u.PublicId != null && u.PublicId != "")
      .Select(u => u.PublicId!)
      .ToListAsync(cancellationToken);

    var used = new HashSet<string>(existing, StringComparer.Ordinal);

    foreach (var u in usersWithoutPublicId)
    {
      string publicId;
      do
      {
        publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.User);
      }
      while (!used.Add(publicId));

      u.SetPublicId(publicId);
    }

    var saved = await _dbContext.SaveChangesAsync(cancellationToken);
    _logBackfilledUsers(_logger, usersWithoutPublicId.Count, saved, null);
  }
}
