namespace ScreenDrafts.Modules.Communications.Features.ContactMessages.SubmitContactMessage;

/// <summary>
/// Verifies Turnstile, stores the message, then emails it to support. The row is saved
/// BEFORE the send so a mail failure never loses the message (EmailedOnUtc stays null).
/// Saves explicitly rather than relying on a pipeline behavior: Communications has no
/// UnitOfWork behavior, unlike Drafts and Reporting.
/// </summary>
internal sealed partial class SubmitContactMessageCommandHandler(
  ITurnstileVerifier turnstileVerifier,
  IContactMessageRepository contactMessageRepository,
  IUnitOfWork unitOfWork,
  IEmailService emailService,
  IDateTimeProvider dateTimeProvider,
  IOptions<ContactOptions> options,
  ILogger<SubmitContactMessageCommandHandler> logger
) : ICommandHandler<SubmitContactMessageCommand>
{
  private readonly ITurnstileVerifier _turnstileVerifier = turnstileVerifier;
  private readonly IContactMessageRepository _contactMessageRepository = contactMessageRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IEmailService _emailService = emailService;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ContactOptions _options = options.Value;
  private readonly ILogger<SubmitContactMessageCommandHandler> _logger = logger;

  public async Task<Result> Handle(
    SubmitContactMessageCommand request,
    CancellationToken cancellationToken
  )
  {
    var verified = await _turnstileVerifier.VerifyAsync(
      request.TurnstileToken,
      request.RemoteIpAddress,
      cancellationToken
    );

    if (!verified)
    {
      return Result.Failure(ContactMessageErrors.VerificationFailed);
    }

    if (!Enum.TryParse<ContactCategory>(request.Category, ignoreCase: true, out var category))
    {
      return Result.Failure(ContactMessageErrors.InvalidCategory);
    }

    var contactMessage = ContactMessage.Create(
      name: request.Name,
      email: request.Email,
      category: category,
      message: request.Message,
      userPublicId: request.UserPublicId,
      createdOnUtc: _dateTimeProvider.UtcNow
    );

    _contactMessageRepository.Add(contactMessage);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    await TryNotifySupportAsync(contactMessage, cancellationToken);

    return Result.Success();
  }

  private async Task TryNotifySupportAsync(
    ContactMessage contactMessage,
    CancellationToken cancellationToken
  )
  {
    var recipient = _options.RecipientAddress;

    if (string.IsNullOrWhiteSpace(recipient))
    {
      LogNoRecipientConfigured(_logger, contactMessage.Id);
      return;
    }

    try
    {
      var senderName = SingleLine(contactMessage.Name);

      var html = EmailTemplates.NewContactMessage(
        name: contactMessage.Name,
        email: contactMessage.Email,
        category: contactMessage.Category.ToString(),
        message: contactMessage.Message,
        userPublicId: contactMessage.UserPublicId,
        createdOnUtc: contactMessage.CreatedOnUtc
      );

      await _emailService.SendAsync(
        new EmailMessage
        {
          ToAddress = recipient,
          ToName = "Screen Drafts Support",
          Subject = $"[Screen Drafts] {contactMessage.Category}: {senderName}",
          HtmlBody = html,
          ReplyToAddress = contactMessage.Email,
          ReplyToName = senderName,
        },
        cancellationToken
      );

      contactMessage.MarkEmailed(_dateTimeProvider.UtcNow);

      await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
#pragma warning disable CA1031 // Intentional: the message is already stored; never fail the submission over mail.
    catch (Exception ex) when (ex is not OperationCanceledException)
#pragma warning restore CA1031
    {
      LogNotificationFailed(_logger, ex, contactMessage.Id);
    }
  }

  // Header values must be a single line; the sender controls this text.
  private static string SingleLine(string value) => value.ReplaceLineEndings(" ");

  [LoggerMessage(
    EventId = 2001,
    Level = LogLevel.Error,
    Message = "Contact message {ContactMessageId} was stored but the support notification email failed."
  )]
  private static partial void LogNotificationFailed(
    ILogger<SubmitContactMessageCommandHandler> logger,
    Exception exception,
    Guid contactMessageId
  );

  [LoggerMessage(
    EventId = 2002,
    Level = LogLevel.Warning,
    Message = "Contact message {ContactMessageId} was stored but no support recipient address is configured."
  )]
  private static partial void LogNoRecipientConfigured(
    ILogger<SubmitContactMessageCommandHandler> logger,
    Guid contactMessageId
  );
}
