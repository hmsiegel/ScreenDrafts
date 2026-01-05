namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;

internal sealed class AddHostToDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IHostsRepository hostsRepository)
  : ICommandHandler<AddHostToDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IHostsRepository _hostsRepository = hostsRepository;

  public async Task<Result<Guid>> Handle(AddHostToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftPartId = DraftPartId.Create(request.DraftPartId);

    var draftPart = await _draftsRepository.GetDraftPartByIdAsync(draftPartId, cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<Guid>(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    var draft = await _draftsRepository.GetDraftByDraftPartId(draftPartId, cancellationToken);
 
    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(draft!.Id.Value));
    }

    var hostId = HostId.Create(request.HostId);

    var host = await _hostsRepository.GetHostByIdAsync(hostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure<Guid>(HostErrors.NotFound(request.HostId));
    }

    var result = request.Role.ToUpperInvariant() switch
    {
      "PRIMARY" => draftPart.SetPrimaryHost(host),
      "CO-HOST" => draftPart.AddCoHost(host),
      _ => Result.Failure<Guid>(DraftErrors.InvalidHostRole(request.Role))
    };

    if (result.IsFailure)
    {
      return Result.Failure<Guid>(result.Errors);
    }

    _draftsRepository.Update(draft);

    return Result.Success(host.Id.Value);
  }
}
