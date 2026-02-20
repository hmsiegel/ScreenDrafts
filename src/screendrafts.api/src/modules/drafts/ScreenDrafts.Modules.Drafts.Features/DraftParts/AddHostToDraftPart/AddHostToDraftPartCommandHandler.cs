using ScreenDrafts.Modules.Drafts.Domain.Hosts;

namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AddHostToDraftPart;

internal sealed class AddHostToDraftPartCommandHandler(
  IDraftPartRepository draftPartRepository,
  IHostRepository hostRepository) : ICommandHandler<AddHostToDraftPartCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IHostRepository _hostRepository = hostRepository;

  public async Task<Result> Handle(AddHostToDraftPartCommand command, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByIdAsync(
      DraftPartId.Create(command.DraftPartId),
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(command.DraftPartId));
    }

    var host = await _hostRepository.GetByPublicIdAsync(command.HostPublicId, cancellationToken);

    if (host is null)
    {
      return Result.Failure(HostErrors.NotFound(command.HostPublicId));
    }

    var result = command.HostRole == HostRole.Primary
      ? draftPart.SetPrimaryHost(host)
      : draftPart.AddCoHost(host);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
