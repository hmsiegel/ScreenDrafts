namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists;

internal sealed class CandidateListEntryAddedIntegrationEventHandler(
  IDraftRepository draftRepository,
  IDraftBoardRepository draftBoardRepository,
  IPublicIdGenerator publicIdGenerator
) : IntegrationEventHandler<CandidateListEntryAddedIntegrationEvent>
{
  public override async Task Handle(
    CandidateListEntryAddedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    var draftId = new DraftId(integrationEvent.DraftId);

    var draft = await draftRepository.GetByIdWithPartsAndParticipantsAsync(
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
      var board = await draftBoardRepository.GetByDraftAndParticipantAsync(
        draftId,
        participant,
        cancellationToken
      );

      if (board is null)
      {
        var publicId = publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftBoard);

        // Candidate list boards are not pool-sourced — participants can add
        // their own movies freely.
        var createResult = DraftBoard.Create(draftId, participant, publicId, isPoolSourced: false);

        if (createResult.IsFailure)
        {
          continue;
        }

        board = createResult.Value;
        draftBoardRepository.Add(board);
      }

      board.SyncAddItem(integrationEvent.TmdbId);
      draftBoardRepository.Update(board);
    }
  }
}
