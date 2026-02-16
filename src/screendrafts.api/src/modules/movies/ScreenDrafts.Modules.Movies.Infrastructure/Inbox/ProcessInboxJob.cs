namespace ScreenDrafts.Modules.Movies.Infrastructure.Inbox;

[DisallowConcurrentExecution]
internal sealed class ProcessInboxJob(
    IDbConnectionFactory dbConnectionFactory,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    IOptions<InboxOptions> inboxOptions,
    ILogger<ProcessInboxJob> logger,
    IIntegrationEventDispatcher integrationEventDispatcher) : IJob
{
  private const string ModuleName = "Movies";

  private readonly IDbConnectionFactory _dbConnectionFactory = dbConnectionFactory;
  private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IIntegrationEventDispatcher _integrationEventDispatcher = integrationEventDispatcher;
  private readonly ILogger<ProcessInboxJob> _logger = logger;
  private readonly InboxOptions _inboxOptions = inboxOptions.Value;

  public async Task Execute(IJobExecutionContext context)
  {
    InboxLoggingMessages.BeginningToProcessInboxMessages(_logger, ModuleName);

    await using DbConnection connection = await _dbConnectionFactory.OpenConnectionAsync();
    await using DbTransaction transaction = await connection.BeginTransactionAsync();

    await CleanupInvalidInboxMessagesAsync(connection, transaction);

    IReadOnlyList<InboxMessageResponse> inboxMessages = await GetInboxMessagesAsync(connection, transaction);

    foreach (var inboxMessage in inboxMessages)
    {
      Exception? exception = null;
      try
      {
        if (string.IsNullOrWhiteSpace(inboxMessage.Content))
        {
          InboxLoggingMessages.EmptyContent(_logger, ModuleName, inboxMessage.Id);
          continue;
        }

        IIntegrationEvent? integrationEvent = null;
        try
        {
          integrationEvent = JsonConvert.DeserializeObject<IIntegrationEvent>(
              inboxMessage.Content,
              SerializerSettings.Instance)!;
        }
        catch (JsonException jsonException)
        {
          InboxLoggingMessages.FailedToDeserialize(
              _logger,
              inboxMessage.Id,
              ModuleName,
              jsonException.Message,
              jsonException);
          exception = jsonException;
        }

        if (integrationEvent is null)
        {
          InboxLoggingMessages.MarkingAsFailed(_logger, inboxMessage.Id, ModuleName);
          await UpdateInboxMessageAsync(connection, transaction, inboxMessage, exception);
          continue;
        }


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
             FROM movies.inbox_messages
             WHERE processed_on_utc IS NULL
             ORDER BY occurred_on_utc
             LIMIT {_inboxOptions.BatchSize}
             FOR UPDATE
             """;

    var inboxMessages = await connection.QueryAsync<InboxMessageResponse>(
        sql,
        transaction: transaction);

    return [.. inboxMessages];
  }

  private async Task UpdateInboxMessageAsync(
      IDbConnection connection,
      IDbTransaction transaction,
      InboxMessageResponse inboxMessage,
      Exception? exception)
  {
    const string sql =
        """
            UPDATE movies.inbox_messages
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

  private async Task CleanupInvalidInboxMessagesAsync(IDbConnection connection, IDbTransaction transaction)
  {
    const string sql =
      $"""
      DELETE FROM movies.inbox_messages
      WHERE "content"::text LIKE '%"Title": null%'
      """;

    var deletedCount = await connection.ExecuteAsync(
      sql,
      transaction: transaction);

    if (deletedCount > 0)
    {
      InboxLoggingMessages.DeletedInboxMessages(_logger, deletedCount, ModuleName);
    }
  }

  internal sealed record InboxMessageResponse(Guid Id, string Content);
}
