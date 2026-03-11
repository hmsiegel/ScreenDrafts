namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftBoards;

internal sealed class VetoAddedDomainEventHandler(
  IDraftBoardRepository draftBoardRepository,
  IDraftPartRepository draftPartRepository,
  ParticipantResolver participantResolver)
  : DomainEventHandler<VetoAddedDomainEvent>
{
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly ParticipantResolver _participantResolver = participantResolver;

  public override async Task Handle(VetoAddedDomainEvent domainEvent, CancellationToken cancellationToken = default)
  {
    var draftPartId = DraftPartId.Create(domainEvent.DraftPartId);

    var draftPart = await _draftPartRepository.GetByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return;
    }

    var participantKind = ParticipantKind.FromValue(domainEvent.ParticipantKind);

    var participant = await _participantResolver.ResolveByParticpantIdAsync(
      domainEvent.ParticipantId,
      participantKind,
      cancellationToken);

    if (participant is null)
    {
      return;
    }

    var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
      draftPart.DraftId,
      participant.Value,
      cancellationToken);

    if (board is null || domainEvent.TmdbId is null)
    {
      return;
    }

    board.AddItem(
      tmdbId: domainEvent.TmdbId.Value,
      notes: null,
      priority: null);
  }
}
