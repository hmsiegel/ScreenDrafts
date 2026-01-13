namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.RemoveHost;

internal sealed class CommandHandler(
  IDraftPartsRepository draftPartsRepository)
    : Common.Features.Abstractions.Messaging.ICommandHandler<Command>
{
  private readonly IDraftPartsRepository _draftPartsRepository = draftPartsRepository;

  public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);
    var hostId = HostId.Create(request.HostId);

    var draftPart = await _draftPartsRepository.GetByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(request.DraftPartId));
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
