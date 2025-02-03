namespace ScreenDrafts.Modules.Audit.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxJob> logger) : IJob
{
  private const string ModuleName = "Audit";

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ILogger<ProcessOutboxJob> _logger = logger;
  private readonly OutboxOptions _outboxOptions = outboxOptions.Value;

  public async Task Execute(IJobExecutionContext context)
  {
    OutboxLoggingMessages.BeginningToProcessOutboxMessages(_logger, ModuleName);

    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    await using DbTransaction transaction = await connection.BeginTransactionAsync();

    IReadOnlyList<OutboxMessageResponse> outboxMessages = await GetOutboxMessagesAsync(connection, transaction);

    foreach (var outboxMessage in outboxMessages)
    {
      Exception? exception = null;
      try
      {
        IDomainEvent domainEvent = JsonConvert.DeserializeObject<IDomainEvent>(
            outboxMessage.Content,
            SerializerSettings.Instance)!;

        using var scope = _serviceScopeFactory.CreateScope();

        IEnumerable<IDomainEventHandler> domainEventHandlers = DomainEventHandlersFactory.GetHandlers(
            domainEvent.GetType(),
            scope.ServiceProvider,
            Application.AssemblyReference.Assembly);

        foreach (IDomainEventHandler domainEventHandler in domainEventHandlers)
        {
          await domainEventHandler.Handle(domainEvent);
        }

      }
      catch (InvalidOperationException caughtException)
      {
        OutboxLoggingMessages.ExceptionWhileProcessingOutboxMessage(
          _logger,
          ModuleName,
          outboxMessage.Id);

        exception = caughtException;
      }

      await UpdateOutboxMessageAsync(connection, transaction, outboxMessage, exception);
    }

    await transaction.CommitAsync();

    OutboxLoggingMessages.CompletedProcessingOutboxMessages(_logger, ModuleName);
  }

  private async Task<IReadOnlyList<OutboxMessageResponse>> GetOutboxMessagesAsync(
      IDbConnection connection,
      IDbTransaction transaction)
  {
    var sql =
        $"""
             SELECT
                id AS {nameof(OutboxMessageResponse.Id)},
                content AS {nameof(OutboxMessageResponse.Content)}
             FROM audit.outbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT {_outboxOptions.BatchSize}
             FOR UPDATE
             """;

    var outboxMessages = await connection.QueryAsync<OutboxMessageResponse>(
        sql,
        transaction: transaction);

    return outboxMessages.ToList();
  }

  private async Task UpdateOutboxMessageAsync(
      IDbConnection connection,
      IDbTransaction transaction,
      OutboxMessageResponse outboxMessage,
      Exception? exception)
  {
    const string sql =
        """
            UPDATE audit.outbox_messages
            SET processed_on_utc = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

    await connection.ExecuteAsync(
        sql,
        new
        {
          outboxMessage.Id,
          ProcessedOnUtc = _dateTimeProvider.UtcNow,
          Error = exception?.ToString()
        },
        transaction: transaction);
  }

  internal sealed record OutboxMessageResponse(Guid Id, string Content);
}
