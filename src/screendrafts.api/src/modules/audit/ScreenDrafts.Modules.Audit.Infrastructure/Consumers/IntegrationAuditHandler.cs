namespace ScreenDrafts.Modules.Audit.Infrastructure.Consumers;

/// <summary>
/// Writes an audit log entry for every integration event it is subscribed to. Rebus delivers by
/// exact message type, so the audit module registers one handler per concrete event type.
/// </summary>
internal sealed partial class IntegrationAuditHandler<TEvent>(
  IAuditWriteService auditWriteService,
  ILogger<IntegrationAuditHandler<TEvent>> logger)
  : IHandleMessages<TEvent>
  where TEvent : IIntegrationEvent
{
  private readonly IAuditWriteService _auditWriteService = auditWriteService;
  private readonly ILogger<IntegrationAuditHandler<TEvent>> _logger = logger;

  public async Task Handle(TEvent message)
  {
    ArgumentNullException.ThrowIfNull(message);

    var transportMessage = MessageContext.Current.TransportMessage;
    transportMessage.Headers.TryGetValue(Headers.MessageId, out var messageId);
    LogHandlerInvoked(_logger, messageId);

    // With pure-JSON serialization the transport body is the event itself, with no envelope.
    var payload = Encoding.UTF8.GetString(transportMessage.Body);
    var eventType = typeof(TEvent).FullName ?? "Unknown";

    var log = new DomainEventAuditLog(
        Id: Guid.NewGuid(),
        OccurredOnUtc: message.OccurredOnUtc,
        EventType: eventType,
        SourceModule: DeriveSourceModule(eventType),
        ActorId: TryGetProperty(message, "ActorId"),
        EntityId: TryGetProperty(message, "PublicId") ?? TryGetProperty(message, "DraftPartPublicId"),
        Payload: payload);

    await _auditWriteService.WriteDomainEventLogAsync(log, CancellationToken.None);
    LogDomainEventLogWritten(_logger, eventType);
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

  private static string? TryGetProperty(TEvent message, string propertyName) =>
    typeof(TEvent)
      .GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)
      ?.GetValue(message) as string;

  [LoggerMessage(
    EventId = 1,
    Level = LogLevel.Information,
    Message = "[Audit] Handler invoked for message {MessageId}")]
  private static partial void LogHandlerInvoked(ILogger logger, string? messageId);

  [LoggerMessage(
    EventId = 2,
    Level = LogLevel.Information,
    Message = "[Audit] Successfully wrote domain event log for {EventType}")]
  private static partial void LogDomainEventLogWritten(ILogger logger, string eventType);
}
