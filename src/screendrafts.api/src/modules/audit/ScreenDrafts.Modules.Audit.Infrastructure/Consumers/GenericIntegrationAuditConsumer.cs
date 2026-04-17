using System.Text;

namespace ScreenDrafts.Modules.Audit.Infrastructure.Consumers;

internal sealed partial class GenericIntegrationAuditConsumer(
  IAuditWriteService auditWriteService,
  ILogger<GenericIntegrationAuditConsumer> logger)
  : IConsumer<IIntegrationEvent>
{
  private readonly IAuditWriteService _auditWriteService = auditWriteService;
  private readonly ILogger<GenericIntegrationAuditConsumer> _logger = logger;

  public async Task Consume(ConsumeContext<IIntegrationEvent> context)
  {
    LogConsumerInvoked(_logger, context.MessageId);

    using var stream = context.ReceiveContext.GetBodyStream();
    using var reader = new StreamReader(stream, Encoding.UTF8);
    var rawEnvelope = await reader.ReadToEndAsync(context.CancellationToken);

    var (eventType, messagePayload) = ParseEnvelope(rawEnvelope, context.Message);
    var sourceModule = DeriveSourceModule(eventType);

    var actorId = TryGetProperty(context.Message, "ActorId");
    var entityId = TryGetProperty(context.Message, "PublicId")
                ?? TryGetProperty(context.Message, "DraftPartPublicId");

    var log = new DomainEventAuditLog(
        Id: Guid.NewGuid(),
        OccurredOnUtc: context.Message.OccurredOnUtc,
        EventType: eventType,
        SourceModule: sourceModule,
        ActorId: actorId,
        EntityId: entityId,
        Payload: messagePayload);
    try
    {
      await _auditWriteService.WriteDomainEventLogAsync(log, context.CancellationToken);
      LogDomainEventLogWritten(_logger, eventType);
    }
    catch (UnhandledEventException ex)
    {
      LogFailedToWriteDomainEventAuditLog(_logger, ex, eventType);
    }
  }

  private static (string EventType, string Payload) ParseEnvelope(
    string rawEnvelope,
    IIntegrationEvent fallback)
  {
    try
    {
      using var doc = JsonDocument.Parse(rawEnvelope);
      var root = doc.RootElement;

      var eventType = ExtractEventType(root, fallback);
      var payload = ExtractMessageBody(root, rawEnvelope);

      return (eventType, payload);
    }
    catch (System.Text.Json.JsonException)
    {
      return (fallback.GetType().FullName ?? "Unknown", rawEnvelope);
    }
  }

  private static string ExtractEventType(JsonElement root, IIntegrationEvent fallback)
  {
    // MassTransit envelope: { "messageType": ["urn:message:Namespace:TypeName", ...] }
    // Take the first entry — it is the most-derived concrete type.
    if (root.TryGetProperty("messageType", out var typeArray)
        && typeArray.ValueKind == JsonValueKind.Array
        && typeArray.GetArrayLength() > 0)
    {
      var urn = typeArray[0].GetString();
      if (!string.IsNullOrWhiteSpace(urn))
      {
        // "urn:message:ScreenDrafts.Modules.Drafts.IntegrationEvents:DraftCreatedIntegrationEvent"
        // → "ScreenDrafts.Modules.Drafts.IntegrationEvents.DraftCreatedIntegrationEvent"
        return urn
            .Replace("urn:message:", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(":", ".", StringComparison.OrdinalIgnoreCase);
      }
    }

    return fallback.GetType().FullName ?? "Unknown";
  }

  private static string ExtractMessageBody(JsonElement root, string rawEnvelope)
  {
    // MassTransit envelope: { "message": { ... actual event properties ... } }
    if (root.TryGetProperty("message", out var message))
    {
      return message.GetRawText();
    }

    return rawEnvelope;
  }

  private static string DeriveSourceModule(string eventType)
  {
    var parts = eventType.Split('.');
    if (parts.Length >= 4 && parts[0] == "ScreenDrafts" && parts[1] == "Modules")
    {
      return parts[2];
    }

    return "Unknown";
  }

  private static string? TryGetProperty(IIntegrationEvent message, string propertyName)
  {
    return message.GetType()
        .GetProperty(propertyName,
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Instance)
        ?.GetValue(message) as string;
  }

  [LoggerMessage(
    EventId = 0,
    Level = LogLevel.Error,
    Message = "Failed to write domain event audit log for event type {EventType}")]
  private static partial void LogFailedToWriteDomainEventAuditLog(
    ILogger logger,
    Exception ex,
    string eventType);

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "[Audit] Consumer invoked for message {MessageId}")]
  private static partial void LogConsumerInvoked(ILogger logger, Guid? messageId);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "[Audit] Successfully wrote domain event log for {EventType}")]
  private static partial void LogDomainEventLogWritten(ILogger logger, string eventType);
}
