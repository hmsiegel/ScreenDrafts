using ScreenDrafts.Common.Infrastructure.Inbox;

namespace ScreenDrafts.Modules.Communications.Infrastructure.Outbox;

internal sealed class IdempotentDomainEventHandler<TDomainEvent>(
    IDomainEventHandler<TDomainEvent> decorated,
    IDbConnectionFactory dbConnectionFactory)
    : DomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
  public override async Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync(cancellationToken);

    var outboxMessageConsumer = new OutboxMessageConsumer(domainEvent.Id, decorated.GetType().Name);

    if (await OutboxConsumerExistsAsync(connection, outboxMessageConsumer))
    {
      return;
    }

    await decorated.Handle(domainEvent, cancellationToken);

    await InsertOutboxConsumerAsync(connection, outboxMessageConsumer);
  }

  private static async Task<bool> OutboxConsumerExistsAsync(
      DbConnection dbConnection,
      OutboxMessageConsumer outboxMessageConsumer)
  {
    const string sql =
        """
            SELECT EXISTS(
                SELECT 1
                FROM communications.outbox_message_consumers
                WHERE outbox_message_id = @OutboxMessageId AND
                      name = @Name
            )
            """;

    return await dbConnection.ExecuteScalarAsync<bool>(sql, outboxMessageConsumer);
  }

  private static async Task InsertOutboxConsumerAsync(
      DbConnection dbConnection,
      OutboxMessageConsumer outboxMessageConsumer)
  {
    const string sql =
        """
            INSERT INTO communications.outbox_message_consumers(outbox_message_id, name)
            VALUES (@OutboxMessageId, @Name)
            """;

    await dbConnection.ExecuteAsync(sql, outboxMessageConsumer);
  }
}
