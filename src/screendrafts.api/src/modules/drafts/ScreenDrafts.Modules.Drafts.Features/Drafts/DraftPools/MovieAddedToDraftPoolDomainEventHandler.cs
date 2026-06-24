namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools;

internal sealed class MovieAddedToDraftPoolDomainEventHandler(
  IDraftRepository draftRepository,
  IDraftBoardRepository draftBoardRepository,
  IPublicIdGenerator publicIdGenerator
) : DomainEventHandler<MovieAddedToDraftPoolDomainEvent>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IDraftBoardRepository _draftBoardRepository = draftBoardRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public override async Task Handle(
    MovieAddedToDraftPoolDomainEvent domainEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draftId = new DraftId(domainEvent.DraftId);

    var draft = await _draftRepository.GetByIdWithPartsAndParticipantsAsync(
      draftId,
      cancellationToken
    );

    if (draft is null)
    {
      return;
    }

    var participants = draft
      .Parts.SelectMany(p => p.Participants)
      .Where(p => p.Kind != ParticipantKind.Community)
      .DistinctBy(p => p.Value)
      .ToList();

    foreach (var participant in participants)
    {
      var board = await _draftBoardRepository.GetByDraftAndParticipantAsync(
        draftId,
        participant,
        cancellationToken
      );

      if (board is null)
      {
        var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftBoard);
        var createResult = DraftBoard.Create(draftId, participant, publicId, isPoolSourced: true);

        if (createResult.IsFailure)
        {
          continue;
        }

        board = createResult.Value;
        _draftBoardRepository.Add(board);
      }

      board.SyncAddItem(domainEvent.TmdbId);
      _draftBoardRepository.Update(board);
    }
  }
}
