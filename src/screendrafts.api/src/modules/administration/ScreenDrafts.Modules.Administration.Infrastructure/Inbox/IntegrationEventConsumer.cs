using Rebus.Handlers;

namespace ScreenDrafts.Modules.Administration.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(
  IDbConnectionFactory dbConnectionFactory
) : IHandleMessages<TIntegrationEvent>
  where TIntegrationEvent : IntegrationEvent
{
  public async Task Handle(TIntegrationEvent message)
  {
    await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

    TIntegrationEvent integrationEvent = message;

    var inboxMessage = new InboxMessage
    {
      Id = integrationEvent.Id,
      Type = integrationEvent.GetType().Name,
      Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
      OccurredOnUtc = integrationEvent.OccurredOnUtc,
    };

    const string sql = """
      INSERT INTO administration.inbox_messages(id, type, content, occurred_on_utc)
      VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
      """;

    await connection.ExecuteAsync(sql, inboxMessage);
  }
}
