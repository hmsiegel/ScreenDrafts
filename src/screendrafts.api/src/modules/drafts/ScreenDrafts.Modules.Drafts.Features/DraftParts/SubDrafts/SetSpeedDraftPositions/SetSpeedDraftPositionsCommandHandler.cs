namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSpeedDraftPositions;

internal sealed class SetSpeedDraftPositionsCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<SetSpeedDraftPositionsCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    SetSpeedDraftPositionsCommand request,
    CancellationToken cancellationToken
  )
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(
      request.DraftPartId,
      cancellationToken
    );

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    // Same config (2 entries: A, B) applied identically to every existing
    // sub-draft — one flat seed list, a fresh public ID per position.
    var seeds =
      new List<(string SubDraftPublicId, string PositionPublicId, string Name, int[] Picks)>();

    foreach (var subDraft in draftPart.SubDrafts)
    {
      foreach (var entry in request.Positions)
      {
        var positionPublicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPosition);
        seeds.Add((subDraft.PublicId, positionPublicId, entry.Name, [.. entry.Picks]));
      }
    }

    var result = draftPart.SetSpeedDraftPositions(seeds);

    if (result.IsFailure)
    {
      return result;
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
