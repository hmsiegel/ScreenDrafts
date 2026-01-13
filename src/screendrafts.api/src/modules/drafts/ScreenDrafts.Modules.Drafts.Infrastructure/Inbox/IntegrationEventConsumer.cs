using ScreenDrafts.Common.Features.Abstractions.Data;
using ScreenDrafts.Common.Features.Abstractions.EventBus;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Inbox;

internal sealed class IntegrationEventConsumer<TIntegrationEvent>(IDbConnectionFactory dbConnectionFactory)
    : IConsumer<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
{
  public async Task Consume(ConsumeContext<TIntegrationEvent> context)
  {
    await using var connection = await dbConnectionFactory.OpenConnectionAsync();

    var integrationEvent = context.Message;

    var inboxMessage = new InboxMessage
    {
      Id = integrationEvent.Id,
      Type = integrationEvent.GetType().Name,
      Content = JsonConvert.SerializeObject(integrationEvent, SerializerSettings.Instance),
      OccurredOnUtc = integrationEvent.OccurredOnUtc
    };

    const string sql =
        """
            INSERT INTO drafts.inbox_messages(id, type, content, occurred_on_utc)
            VALUES (@Id, @Type, @Content::json, @OccurredOnUtc)
            ON CONFLICT (id) DO NOTHING
            """;

    await connection.ExecuteAsync(sql, inboxMessage);
  }
}
