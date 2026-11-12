using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenDrafts.Modules.Movies.Features.Movies.SyncMediaPeople;

namespace ScreenDrafts.Modules.Movies.Features.Movies.ResyncMediaPeople;

/// <summary>
/// Re-fetches cast and crew from TMDb for titles that still lack them (or lack billing order)
/// and attaches them, one title at a time. One run at a time, in the background of the API
/// process. The state lives in memory: a restart ends the run, and starting again resumes it
/// because finished titles no longer match the selection.
/// </summary>
internal sealed partial class MediaPeopleResyncRunner(
  IServiceScopeFactory scopeFactory,
  IHostApplicationLifetime applicationLifetime,
  ILogger<MediaPeopleResyncRunner> logger
)
{
  internal const string Idle = "Idle";
  internal const string Running = "Running";
  internal const string Completed = "Completed";
  internal const string Cancelled = "Cancelled";
  internal const string Aborted = "Aborted";
  internal const string Failed = "Failed";

  private const int ProgressInterval = 25;
  private const int MaxConsecutiveFailures = 10;

  private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
  private readonly IHostApplicationLifetime _applicationLifetime = applicationLifetime;
  private readonly ILogger<MediaPeopleResyncRunner> _logger = logger;

  private readonly object _gate = new();
  private CancellationTokenSource? _cts;
  private string _state = Idle;
  private int _total;
  private int _succeeded;
  private int _failed;
  private DateTimeOffset? _startedAt;
  private DateTimeOffset? _finishedAt;
  private string? _stopReason;

  internal bool IsRunning
  {
    get
    {
      lock (_gate)
      {
        return _state == Running;
      }
    }
  }

  internal MediaPeopleResyncStatusResponse GetStatus()
  {
    lock (_gate)
    {
      var done = _succeeded + _failed;
      double? perMinute = null;
      string? eta = null;

      if (_startedAt is not null && done > 0)
      {
        var end = _finishedAt ?? TimeProvider.System.GetUtcNow();
        var minutes = (end - _startedAt.Value).TotalMinutes;
        if (minutes > 0)
        {
          perMinute = Math.Round(done / minutes, 1);
          if (_state == Running && perMinute > 0)
          {
            eta = FormatEta(TimeSpan.FromMinutes((_total - done) / perMinute.Value));
          }
        }
      }

      return new MediaPeopleResyncStatusResponse
      {
        State = _state,
        Total = _total,
        Succeeded = _succeeded,
        Failed = _failed,
        PerMinute = perMinute,
        Eta = eta,
        StartedAt = _startedAt,
        FinishedAt = _finishedAt,
        StopReason = _stopReason,
      };
    }
  }

  /// <summary>Starts a run unless one is already in progress. Returns false if one is.</summary>
  /// <param name="limit">Stop after this many titles; null means every pending title.</param>
  internal bool TryStart(int? limit)
  {
    CancellationTokenSource cts;

    lock (_gate)
    {
      if (_state == Running)
      {
        return false;
      }

      _cts?.Dispose();
      cts = CancellationTokenSource.CreateLinkedTokenSource(
        _applicationLifetime.ApplicationStopping
      );
      _cts = cts;
      _state = Running;
      _total = 0;
      _succeeded = 0;
      _failed = 0;
      _startedAt = TimeProvider.System.GetUtcNow();
      _finishedAt = null;
      _stopReason = null;
    }

    // RunAsync handles every exception itself, so nothing is left unobserved.
    _ = Task.Run(() => RunAsync(limit, cts.Token), CancellationToken.None);
    return true;
  }

  /// <summary>Asks a run in progress to stop after the title it is on. False if none is running.</summary>
  internal bool Cancel()
  {
    lock (_gate)
    {
      if (_state != Running || _cts is null)
      {
        return false;
      }

      _cts.Cancel();
      return true;
    }
  }

  private async Task RunAsync(int? limit, CancellationToken cancellationToken)
  {
#pragma warning disable CA1031 // Do not catch general exception types
    try
    {
      var candidates = await LoadCandidatesAsync(limit, cancellationToken);

      lock (_gate)
      {
        _total = candidates.Count;
      }

      if (candidates.Count == 0)
      {
        LogNothingToResync(_logger);
        Finish(Completed, null);
        return;
      }

      LogStarting(_logger, candidates.Count);

      var stopwatch = System.Diagnostics.Stopwatch.StartNew();
      var consecutiveFailures = 0;

      foreach (var item in candidates)
      {
        cancellationToken.ThrowIfCancellationRequested();

        var (done, failedSoFar) = Counts();
        if (done > 0 && done % ProgressInterval == 0)
        {
          // Locals, not inline expressions: CA1873 rejects expensive log arguments.
          var perMinute = Math.Round(done / stopwatch.Elapsed.TotalMinutes, 1);
          var eta = TimeSpan.FromMinutes((candidates.Count - done) / Math.Max(perMinute, 0.1));
          var etaText = FormatEta(eta);
          LogProgress(_logger, done, candidates.Count, failedSoFar, perMinute, etaText);
        }

        bool succeeded;
        try
        {
          succeeded = await ProcessAsync(item, cancellationToken);
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
          // One bad title (a TMDb hiccup, a database timeout, an HttpClient timeout that
          // surfaces as a cancellation) must not end the run.
          LogException(_logger, item.PublicId, ex.ToString());
          succeeded = false;
        }

        lock (_gate)
        {
          if (succeeded)
          {
            _succeeded++;
          }
          else
          {
            _failed++;
          }
        }

        consecutiveFailures = succeeded ? 0 : consecutiveFailures + 1;

        if (consecutiveFailures >= MaxConsecutiveFailures)
        {
          LogAborting(_logger, consecutiveFailures);
          Finish(
            Aborted,
            $"Stopped after {consecutiveFailures} consecutive failures; the database or TMDb is likely down."
          );
          return;
        }
      }

      var (finalDone, finalFailed) = Counts();
      var finalSucceeded = finalDone - finalFailed;
      LogComplete(_logger, finalSucceeded, finalFailed);
      Finish(Completed, null);
    }
    catch (OperationCanceledException)
    {
      var shuttingDown = _applicationLifetime.ApplicationStopping.IsCancellationRequested;
      LogCancelled(_logger, shuttingDown);
      Finish(
        Cancelled,
        shuttingDown
          ? "The application is shutting down. Start again to resume."
          : "Cancelled. Start again to resume."
      );
    }
    catch (Exception ex)
    {
      LogRunFailed(_logger, ex.ToString());
      Finish(Failed, ex.Message);
    }
#pragma warning restore CA1031 // Do not catch general exception types
  }

  private async Task<List<Candidate>> LoadCandidatesAsync(
    int? limit,
    CancellationToken cancellationToken
  )
  {
    await using var scope = _scopeFactory.CreateAsyncScope();
    var connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
    await using var connection = await connectionFactory.OpenConnectionAsync(cancellationToken);

    var rows = await connection.QueryAsync<Candidate>(
      MediaPeopleResyncSql.Candidates,
      new { VideoGame = MediaType.VideoGame.Value, MusicVideo = MediaType.MusicVideo.Value }
    );

    var all = rows.ToList();
    return limit is > 0 ? [.. all.Take(limit.Value)] : all;
  }

  /// <summary>
  /// Fetches one title's credits from TMDb and attaches them. Each title gets its own scope
  /// and so its own DbContext: a failure cannot leave half-built entities for the next save.
  /// </summary>
  private async Task<bool> ProcessAsync(Candidate item, CancellationToken cancellationToken)
  {
    await using var scope = _scopeFactory.CreateAsyncScope();
    var integrations = scope.ServiceProvider.GetRequiredService<IIntegrationsApi>();
    var sender = scope.ServiceProvider.GetRequiredService<ISender>();

    var fetchResult = await integrations.GetOnlineMediaCreditsAsync(
      new OnlineMediaCreditsApiRequest
      {
        MediaType = item.MediaTypeValue,
        TmdbId = item.TmdbId,
        ImdbId = item.ImdbId,
        TvSeriesTmdbId = item.TvSeriesTmdbId,
        SeasonNumber = item.SeasonNumber,
        EpisodeNumber = item.EpisodeNumber,
      },
      cancellationToken
    );

    if (fetchResult.IsFailure)
    {
      LogFetchFailed(_logger, item.PublicId);
      return false;
    }

    var credits = fetchResult.Value;

    var syncResult = await sender.Send(
      new SyncMediaPeopleCommand
      {
        TmdbId = item.TmdbId,
        MediaType = MediaType.FromValue(item.MediaTypeValue),
        TvSeriesTmdbId = item.TvSeriesTmdbId,
        SeasonNumber = item.SeasonNumber,
        EpisodeNumber = item.EpisodeNumber,
        Directors =
        [
          .. credits.Directors.Select(d => new PersonRequest(d.Name, d.ImdbId, d.TmdbId)),
        ],
        Actors = [.. credits.Actors.Select(a => new PersonRequest(a.Name, a.ImdbId, a.TmdbId))],
        Writers = [.. credits.Writers.Select(w => new PersonRequest(w.Name, w.ImdbId, w.TmdbId))],
        Producers =
        [
          .. credits.Producers.Select(p => new PersonRequest(p.Name, p.ImdbId, p.TmdbId)),
        ],
        ProductionCompanies =
        [
          .. credits.ProductionCompanies.Select(c => new ProductionCompanyRequest(
            c.Name,
            c.ImdbId,
            c.TmdbId
          )),
        ],
      },
      cancellationToken
    );

    if (syncResult.IsFailure)
    {
      var error = syncResult.Error?.ToString() ?? string.Empty;
      LogSyncFailed(_logger, item.PublicId, error);
      return false;
    }

    return true;
  }

  private (int Done, int Failed) Counts()
  {
    lock (_gate)
    {
      return (_succeeded + _failed, _failed);
    }
  }

  private void Finish(string state, string? stopReason)
  {
    lock (_gate)
    {
      _state = state;
      _stopReason = stopReason;
      _finishedAt = TimeProvider.System.GetUtcNow();
    }
  }

  private static string FormatEta(TimeSpan eta) => $"{(int)eta.TotalHours}h {eta.Minutes:00}m";

  private sealed record Candidate(
    string PublicId,
    int TmdbId,
    string? ImdbId,
    int MediaTypeValue,
    int? TvSeriesTmdbId,
    int? SeasonNumber,
    int? EpisodeNumber
  );

  [LoggerMessage(
    EventId = 1201,
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: no pending titles; nothing to do."
  )]
  private static partial void LogNothingToResync(ILogger<MediaPeopleResyncRunner> logger);

  [LoggerMessage(
    EventId = 1202,
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: starting resync for {Count} titles."
  )]
  private static partial void LogStarting(ILogger<MediaPeopleResyncRunner> logger, int count);

  [LoggerMessage(
    EventId = 1203,
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: {Done}/{Total} done, {Failed} failed, {PerMinute}/min, about {Eta} left."
  )]
  private static partial void LogProgress(
    ILogger<MediaPeopleResyncRunner> logger,
    int done,
    int total,
    int failed,
    double perMinute,
    string eta
  );

  [LoggerMessage(
    EventId = 1204,
    Level = LogLevel.Warning,
    Message = "MediaPeopleResync: failed to fetch TMDb credits for {PublicId}; skipping."
  )]
  private static partial void LogFetchFailed(
    ILogger<MediaPeopleResyncRunner> logger,
    string publicId
  );

  [LoggerMessage(
    EventId = 1205,
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: sync failed for {PublicId}. Error: {Error}"
  )]
  private static partial void LogSyncFailed(
    ILogger<MediaPeopleResyncRunner> logger,
    string publicId,
    string error
  );

  [LoggerMessage(
    EventId = 1206,
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: exception processing {PublicId}. Details: {Details}"
  )]
  private static partial void LogException(
    ILogger<MediaPeopleResyncRunner> logger,
    string publicId,
    string details
  );

  [LoggerMessage(
    EventId = 1207,
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: aborting after {Count} consecutive failures (database or TMDb likely down)."
  )]
  private static partial void LogAborting(ILogger<MediaPeopleResyncRunner> logger, int count);

  [LoggerMessage(
    EventId = 1208,
    Level = LogLevel.Information,
    Message = "MediaPeopleResync: complete. Succeeded: {Succeeded}, Failed: {Failed}."
  )]
  private static partial void LogComplete(
    ILogger<MediaPeopleResyncRunner> logger,
    int succeeded,
    int failed
  );

  [LoggerMessage(
    EventId = 1209,
    Level = LogLevel.Warning,
    Message = "MediaPeopleResync: stopped early. Application shutting down: {ShuttingDown}."
  )]
  private static partial void LogCancelled(
    ILogger<MediaPeopleResyncRunner> logger,
    bool shuttingDown
  );

  [LoggerMessage(
    EventId = 1210,
    Level = LogLevel.Error,
    Message = "MediaPeopleResync: the run failed. Details: {Details}"
  )]
  private static partial void LogRunFailed(ILogger<MediaPeopleResyncRunner> logger, string details);
}
