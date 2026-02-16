namespace ScreenDrafts.Modules.Administration.Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    ILogger<ProcessInboxJob> logger,
    IIntegrationEventDispatcher integrationEventDispatcher) : IJob
{
  private const string ModuleName = "Administration";

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ILogger<ProcessInboxJob> _logger = logger;
  private readonly InboxOptions _inboxOptions = inboxOptions.Value;
  private readonly IIntegrationEventDispatcher _integrationEventDispatcher = integrationEventDispatcher;

  public async Task Execute(IJobExecutionContext context)
  {
    InboxLoggingMessages.BeginningToProcessInboxMessages(_logger, ModuleName);

    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    await using DbTransaction transaction = await connection.BeginTransactionAsync();

    IReadOnlyList<InboxMessageResponse> inboxMessages = await GetInboxMessagesAsync(connection, transaction);

    foreach (var inboxMessage in inboxMessages)
    {
      Exception? exception = null;
      try
      {
        IIntegrationEvent integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
            inboxMessage.Content,
            SerializerSettings.Instance)!;

        using var scope = _serviceScopeFactory.CreateScope();

        await _integrationEventDispatcher.DispatchAsync(
            integrationEvent,
            scope.ServiceProvider);

      }
      catch (InvalidOperationException caughtException)
      {
        InboxLoggingMessages.ExceptionWhileProcessingInboxMessage(
          _logger,
          ModuleName,
          inboxMessage.Id);

        exception = caughtException;
      }

      await UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
    }

    await transaction.CommitAsync();

    InboxLoggingMessages.CompletedProcessingInboxMessages(_logger, ModuleName);
  }

  private async Task<IReadOnlyList<InboxMessageResponse>> GetInboxMessagesAsync(
      IDbConnection connection,
      IDbTransaction transaction)
  {
    var sql =
        $"""
             SELECT
                id AS {nameof(InboxMessageResponse.Id)},
                content AS {nameof(InboxMessageResponse.Content)}
             FROM administration.inbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT {_inboxOptions.BatchSize}
             FOR UPDATE
             """;

    var inboxMessages = await connection.QueryAsync<InboxMessageResponse>(
        sql,
        transaction: transaction);

    return inboxMessages.ToList();
  }

  private async Task UpdateInboxMessageAsync(
      IDbConnection connection,
      IDbTransaction transaction,
      InboxMessageResponse inboxMessage,
      Exception? exception)
  {
    const string sql =
        """
            UPDATE administration.inbox_messages
            SET processed_on_utc = @ProcessedOnUtc,
                error = @Error
            WHERE id = @Id
            """;

    await connection.ExecuteAsync(
        sql,
        new
        {
          inboxMessage.Id,
          ProcessedOnUtc = _dateTimeProvider.UtcNow,
          Error = exception?.ToString()
        },
        transaction: transaction);
  }

  internal sealed record InboxMessageResponse(Guid Id, string Content);
}
