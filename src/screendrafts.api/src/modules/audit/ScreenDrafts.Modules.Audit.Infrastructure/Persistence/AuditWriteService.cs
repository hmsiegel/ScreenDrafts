namespace ScreenDrafts.Modules.Audit.Infrastructure.Persistence;

internal sealed class AuditWriteService(
  IDbConnectionFactory connectionFactory,
  MongoAuditRepository mongoAuditRepository,
  ILogger<AuditWriteService> logger) : IAuditWriteService
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly MongoAuditRepository _mongoAuditRepository = mongoAuditRepository;
  private readonly ILogger<AuditWriteService> _logger = logger;

  private const string InsertHttpLogQuery =
    """
    INSERT INTO audit.http_audit_logs (id, correlation_id, occurred_on_utc, actor_id, endpoint_name, http_method, route, status_code, duration_ms, request_body, response_body, ip_address)
    VALUES (@Id, @CorrelationId, @OccurredOnUtc, @ActorId, @EndpointName, @HttpMethod, @Route, @StatusCode, @DurationMs, @RequestBody::jsonb, @ResponseBody::jsonb, @IpAddress)
    ON CONFLICT DO NOTHING;
    """;

  private const string InsertDomainEventLogQuery =
    """
    INSERT INTO audit.domain_event_audit_logs (id, occurred_on_utc, event_type, source_module, actor_id, entity_id, payload)
    VALUES (@Id, @OccurredOnUtc, @EventType, @SourceModule, @ActorId, @EntityId, @Payload::jsonb)
    ON CONFLICT DO NOTHING;
    """;

  private const string InsertAuthLog =
    """
    INSERT INTO audit.auth_audit_logs (id, occurred_on_utc, event_type, user_id, client_id, ip_address, details)
    VALUES (@Id, @OccurredOnUtc, @EventType, @UserId, @ClientId, @IpAddress, @Details::jsonb)
    ON CONFLICT DO NOTHING;
    """;

  private static readonly Action<ILogger, Guid, Exception?> LogMongoHttpWriteError =
    LoggerMessage.Define<Guid>(
      LogLevel.Error,
      new EventId(1, nameof(WriteHttpLogAsync)),
      "Failed to write HTTP audit log with ID {LogId} to MongoDB");

  private static readonly Action<ILogger, Guid, Exception?> LogMongoDomainEventWriteError =
    LoggerMessage.Define<Guid>(
      LogLevel.Error,
      new EventId(2, nameof(WriteDomainEventLogAsync)),
      "Failed to write domain event audit log with ID {LogId} to MongoDB");

  private static readonly Action<ILogger, Guid, Exception?> LogMongoAuthWriteError =
    LoggerMessage.Define<Guid>(
      LogLevel.Error,
      new EventId(3, nameof(WriteAuthLogAsync)),
      "Failed to write auth audit log with ID {LogId} to MongoDB");

  private static readonly Action<ILogger, Guid, Exception?> LogHttpAuditWrite =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(4, nameof(WriteHttpLogAsync)),
      "Writing HTTP audit log with ID {LogId} to the database");

  private static readonly Action<ILogger, Guid, Exception?> LogDomainEventAuditWrite =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(5, nameof(WriteDomainEventLogAsync)),
      "Writing domain event audit log with ID {LogId} to the database");

  private static readonly Action<ILogger, Guid, Exception?> LogAuthAuditWrite =
    LoggerMessage.Define<Guid>(
      LogLevel.Information,
      new EventId(6, nameof(WriteAuthLogAsync)),
      "Writing auth audit log with ID {LogId} to the database");

  public async Task WriteHttpLogAsync(HttpAuditLog log, CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    LogHttpAuditWrite(_logger, log.Id, null);

    await connection.ExecuteAsync(
      new CommandDefinition(
        InsertHttpLogQuery,
        new
        {
          log.Id,
          log.CorrelationId,
          log.OccurredOnUtc,
          log.ActorId,
          log.EndpointName,
          log.HttpMethod,
          log.Route,
          log.StatusCode,
          log.DurationMs,
          log.RequestBody,
          log.ResponseBody,
          log.IpAddress
        },
        cancellationToken: cancellationToken
      ));

    // Also write to MongoDB for more complex querying capabilities
    _ = _mongoAuditRepository.WriteHttpLogAsync(log, CancellationToken.None)
        .ContinueWith(
            t => LogMongoHttpWriteError(_logger, log.Id, t.Exception),
            CancellationToken.None,
            TaskContinuationOptions.OnlyOnFaulted,
            TaskScheduler.Default);
  }

  public async Task WriteDomainEventLogAsync(DomainEventAuditLog log, CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    LogDomainEventAuditWrite(_logger, log.Id, null);

    await connection.ExecuteAsync(
      new CommandDefinition(
        InsertDomainEventLogQuery,
        new
        {
          log.Id,
          log.OccurredOnUtc,
          log.EventType,
          log.SourceModule,
          log.ActorId,
          log.EntityId,
          log.Payload
        },
        cancellationToken: cancellationToken
      ));

    // Also write to MongoDB for more complex querying capabilities
    _ = _mongoAuditRepository.WriteDomainEventLogAsync(log, CancellationToken.None)
        .ContinueWith(t =>
            LogMongoDomainEventWriteError(_logger, log.Id, t.Exception),
        CancellationToken.None,
        TaskContinuationOptions.OnlyOnFaulted,
        TaskScheduler.Default);
  }

  public async Task WriteAuthLogAsync(AuthAuditLog log, CancellationToken cancellationToken = default)
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    LogAuthAuditWrite(_logger, log.Id, null);

    await connection.ExecuteAsync(
      new CommandDefinition(
        InsertAuthLog,
        new
        {
          log.Id,
          log.OccurredOnUtc,
          log.EventType,
          log.UserId,
          log.ClientId,
          log.IpAddress,
          log.Details
        },
        cancellationToken: cancellationToken
      ));

    _ = _mongoAuditRepository.WriteAuthLogAsync(log, CancellationToken.None)
        .ContinueWith(t =>
            LogMongoAuthWriteError(_logger, log.Id, t.Exception),
        CancellationToken.None,
        TaskContinuationOptions.OnlyOnFaulted,
        TaskScheduler.Default);
  }
}
