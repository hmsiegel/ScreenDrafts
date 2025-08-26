namespace ScreenDrafts.Modules.Drafts.Application.Drafts.Commands.AddHostToDraft;

internal sealed class AddHostToDraftCommandHandler(
  IDraftsRepository draftsRepository,
  IHostsRepository hostsRepository)
  : ICommandHandler<AddHostToDraftCommand, Guid>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IHostsRepository _hostsRepository = hostsRepository;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1308:Normalize strings to uppercase", Justification = "<Pending>")]
  public async Task<Result<Guid>> Handle(AddHostToDraftCommand request, CancellationToken cancellationToken)
  {
    var draftId = DraftId.Create(request.DraftId);

    var draft = await _draftsRepository.GetByIdAsync(draftId, cancellationToken);
 
    if (draft is null)
    {
      return Result.Failure<Guid>(DraftErrors.NotFound(request.DraftId));
    }

    var hostId = HostId.Create(request.HostId);

    var host = await _hostsRepository.GetHostByIdAsync(hostId, cancellationToken);

    if (host is null)
    {
      return Result.Failure<Guid>(HostErrors.NotFound(request.HostId));
    }

    var result = request.Role.ToLowerInvariant() switch
    {
      "primary" => draft.SetPrimaryHost(host),
      "co-host" => draft.AddCoHost(host),
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
