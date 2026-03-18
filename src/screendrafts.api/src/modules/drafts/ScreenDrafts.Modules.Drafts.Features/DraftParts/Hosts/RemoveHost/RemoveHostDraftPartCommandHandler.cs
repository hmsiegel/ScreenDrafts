namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.Hosts.RemoveHost;

internal sealed class RemoveHostDraftPartCommandHandler(
  IDraftPartRepository draftPartsRepository,
  IHostRepository hostRepository)
    : ICommandHandler<RemoveHostDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartsRepository = draftPartsRepository;
  private readonly IHostRepository _hostRepository = hostRepository;

  public async Task<Result> Handle(RemoveHostDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartsRepository.GetByPublicIdAsync(request.DraftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartId));
    }

    var host = await _hostRepository.GetByPublicIdAsync(request.HostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure(HostErrors.NotFound(request.HostId));
    }

    var result = draftPart.RemoveHost(host.Id);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartsRepository.Update(draftPart);

    return Result.Success();
  }
}
