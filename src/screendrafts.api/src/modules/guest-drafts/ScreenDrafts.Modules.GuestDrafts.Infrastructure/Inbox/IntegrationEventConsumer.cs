using Rebus.Handlers;

namespace ScreenDrafts.Modules.GuestDrafts.Infrastructure.Inbox;

public sealed class IntegrationEventConsumer<TIntegrationEvent>(
  IDbConnectionFactory dbConnectionFactory
) : IHandleMessages<TIntegrationEvent>
  where TIntegrationEvent : IntegrationEvent
{
  public async Task Handle(TIntegrationEvent message)
  {
    ArgumentNullException.ThrowIfNull(message);

    await using var connection = await dbConnectionFactory.OpenConnectionAsync();

    var integrationEvent = message;

    var inboxMessage = new InboxMessage
    {
      Id = integrationEvent.Id,
      Type = integrationEvent.GetType().Name,
      Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
      OccurredOnUtc = integrationEvent.OccurredOnUtc,
    };

    const string sql = """
      INSERT INTO guest_drafts.inbox_messages(id, type, content, occurred_on_utc)
      VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
      ON CONFLICT (id) DO NOTHING
      """;

    await connection.ExecuteAsync(sql, inboxMessage);
  }
}
