namespace ScreenDrafts.Modules.GuestDrafts.Features.Invitations;

internal sealed class InvitationSentDomainEventHandler(
  IEventBus eventBus,
  IDateTimeProvider dateTimeProvider,
  IDraftRepository draftRepository,
  IUsersApi usersApi,
  IOptions<InvitationOptions> options
) : DomainEventHandler<InvitationSentDomainEvent>
{
  private readonly IEventBus _eventBus = eventBus;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUsersApi _usersApi = usersApi;
  private readonly InvitationOptions _options = options.Value;

  public override async Task Handle(
    InvitationSentDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    ArgumentNullException.ThrowIfNull(domainEvent);

    if (string.IsNullOrWhiteSpace(_options.RegisterBaseUrl))
    {
      throw new InvalidOperationException(
        $"{InvitationOptions.SectionName}:RegisterBaseUrl is not configured."
      );
    }

    var draft = await _draftRepository.GetByPublicIdAsync(
      domainEvent.DraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      return;
    }

    var inviter = await _usersApi.GetUserById(domainEvent.InvitedByUserId, cancellationToken);

    var invitedByName = inviter is null
      ? "a Screen Drafts member"
      : $"{inviter.FirstName} {inviter.LastName}".Trim();

    var registerLink =
      $"{_options.RegisterBaseUrl}"
      + $"?name={Uri.EscapeDataString(domainEvent.Name)}"
      + $"&email={Uri.EscapeDataString(domainEvent.Email)}";

    await _eventBus.PublishAsync(
      new GuestDraftInvitationSentIntegrationEvent(
        id: Guid.NewGuid(),
        occurredOnUtc: _dateTimeProvider.UtcNow,
        recipientEmail: domainEvent.Email,
        recipientName: domainEvent.Name,
        draftTitle: draft.Title,
        invitedByName: invitedByName,
        registerLink: registerLink
      ),
      cancellationToken
    );
  }
}
