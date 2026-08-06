namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts;

internal sealed partial class AllSubDraftsCompletedDomainEventHandler(
  IDraftRepository draftRepository,
  IUnitOfWork unitOfWork,
  IDateTimeProvider dateTimeProvider,
  ILogger<AllSubDraftsCompletedDomainEventHandler> logger
) : DomainEventHandler<AllSubDraftsCompletedDomainEvent>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
  private readonly ILogger<AllSubDraftsCompletedDomainEventHandler> _logger = logger;

  public override async Task Handle(
    AllSubDraftsCompletedDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draft = await _draftRepository.GetDraftByPublicIdWithPartsAsync(
      domainEvent.DraftPublicId,
      cancellationToken
    );

    if (draft is null)
    {
      LogDraftNotFound(_logger, domainEvent.DraftPublicId);
      return;
    }

    var result = draft.CompletePart(
      DraftPartId.Create(domainEvent.DraftPartId),
      _dateTimeProvider.UtcNow
    );

    if (result.IsFailure)
    {
      LogCompletePartFailed(
        _logger,
        domainEvent.DraftPartPublicId,
        result.Error?.Code ?? "unknown"
      );
      return;
    }

    _draftRepository.Update(draft);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }

  [LoggerMessage(0, LogLevel.Warning, "AllSubDraftsCompleted — draft {DraftPublicId} not found")]
  private static partial void LogDraftNotFound(ILogger logger, string draftPublicId);

  [LoggerMessage(
    1,
    LogLevel.Warning,
    "AllSubDraftsCompleted — CompletePart failed for draft part {DraftPartPublicId}: {ErrorCode}"
  )]
  private static partial void LogCompletePartFailed(
    ILogger logger,
    string draftPartPublicId,
    string errorCode
  );
}
