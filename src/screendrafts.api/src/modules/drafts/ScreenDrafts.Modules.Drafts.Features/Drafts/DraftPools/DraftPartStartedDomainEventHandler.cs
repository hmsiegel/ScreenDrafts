namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools;

internal sealed class DraftPartStartedDomainEventHandler(
  IDraftPoolRepository draftPoolRepository,
  IUnitOfWork unitOfWork)
  : DomainEventHandler<DraftPartStartedDomainEvent>
{
  private readonly IDraftPoolRepository _draftPoolRepository = draftPoolRepository;
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public override async Task Handle(DraftPartStartedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    var pool = await _draftPoolRepository.GetByDraftIdAsync(
      DraftId.Create(domainEvent.DraftId),
      cancellationToken);

    if (pool is null)
    {
      return;
    }

    pool.Lock();
    _draftPoolRepository.Update(pool);

    await _unitOfWork.SaveChangesAsync(cancellationToken);
  }
}
