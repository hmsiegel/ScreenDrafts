namespace ScreenDrafts.Modules.Communications.Features.Email;

/// <summary>
/// Emails the owner whenever a user registers (password or social). Deliberately
/// separate from <see cref="UserRegisteredIntegrationEventConsumer"/>, and it never
/// throws: an SMTP failure here must not abort the dispatcher loop and skip the
/// user_emails upsert, nor trigger an inbox retry that re-sends the email.
/// </summary>
internal sealed partial class UserRegisteredOwnerNotificationIntegrationEventConsumer(
  IDbConnectionFactory connectionFactory,
  IEmailService emailService,
  IDateTimeProvider dateTimeProvider,
  IOptions<OwnerNotificationOptions> options,
  ILogger<UserRegisteredOwnerNotificationIntegrationEventConsumer> logger
) : IntegrationEventHandler<UserRegisteredIntegrationEvent>
{
  private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
  private readonly IEmailService _emailService = emailService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly OwnerNotificationOptions _options = options.Value;
  private readonly ILogger<UserRegisteredOwnerNotificationIntegrationEventConsumer> _logger =
    logger;

  public override async Task Handle(
    UserRegisteredIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(integrationEvent);

    var ownerAddress = _options.Address;

    if (string.IsNullOrWhiteSpace(ownerAddress))
    {
      return;
    }

    try
    {
      await NotifyOwnerAsync(integrationEvent, ownerAddress, cancellationToken);
    }
#pragma warning disable CA1031 // Intentional: a notification failure must never fail the dispatch.
    catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
    {
      LogOwnerNotificationFailed(_logger, ex, integrationEvent.UserId);
    }
  }

  private async Task NotifyOwnerAsync(
    UserRegisteredIntegrationEvent integrationEvent,
    string ownerAddress,
    CancellationToken cancellationToken
  )
  {
    await using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

    const string alreadySentSql = """
      SELECT email_address
      FROM communications.email_deliveries
      WHERE event_id = @EventId
        AND email_address = @EmailAddress
      LIMIT 1
      """;

    var alreadySent = await connection.QueryFirstOrDefaultAsync<string>(
      new CommandDefinition(
        commandText: alreadySentSql,
        parameters: new { EventId = integrationEvent.Id, EmailAddress = ownerAddress },
        cancellationToken: cancellationToken
      )
    );

    if (alreadySent is not null)
    {
      return;
    }

    var fullName = $"{integrationEvent.FirstName} {integrationEvent.LastName}";

    var html = EmailTemplates.NewUserRegistered(
      userName: fullName,
      userEmail: integrationEvent.Email,
      registeredOnUtc: integrationEvent.OccurredOnUtc
    );

    await _emailService.SendAsync(
      new EmailMessage
      {
        ToAddress = ownerAddress,
        ToName = "Screen Drafts Admin",
        Subject = $"New Screen Drafts registration: {fullName}",
        HtmlBody = html,
      },
      cancellationToken: cancellationToken
    );

    const string recordDeliverySql = """
      INSERT INTO communications.email_deliveries (event_id, email_address, sent_on_utc)
      VALUES (@EventId, @EmailAddress, @SentOnUtc)
      ON CONFLICT DO NOTHING
      """;

    await connection.ExecuteAsync(
      new CommandDefinition(
        commandText: recordDeliverySql,
        parameters: new
        {
          EventId = integrationEvent.Id,
          EmailAddress = ownerAddress,
          SentOnUtc = _dateTimeProvider.UtcNow,
        },
        cancellationToken: cancellationToken
      )
    );
  }

  [LoggerMessage(
    EventId = 1001,
    Level = LogLevel.Error,
    Message = "Failed to send owner registration notification for user {UserId}"
  )]
  private static partial void LogOwnerNotificationFailed(
    ILogger<UserRegisteredOwnerNotificationIntegrationEventConsumer> logger,
    Exception exception,
    Guid userId
  );
}
