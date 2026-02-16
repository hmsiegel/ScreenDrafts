using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class RemoveHostDraftPartCommandHandler(
  IDraftPartRepository draftPartsRepository)
    : ICommandHandler<RemoveHostDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartsRepository = draftPartsRepository;

  public async Task<Result> Handle(RemoveHostDraftPartCommand RemoveHostDraftPartRequest, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(RemoveHostDraftPartRequest.DraftPartId);
    var hostId = HostId.Create(RemoveHostDraftPartRequest.HostId);

    var draftPart = await _draftPartsRepository.GetByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(RemoveHostDraftPartRequest.DraftPartId));
    }

    var result = draftPart.RemoveHost(hostId);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartsRepository.Update(draftPart);

    return Result.Success();
  }
}



