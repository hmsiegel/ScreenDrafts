using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Enums;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.ValueObjects;

namespace ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Commands.AddReleaseToDraftPart;

internal sealed class AddReleaseToDraftPartCommandHandler(
  IDraftRepository draftRepository)
  : ICommandHandler<AddReleaseToDraftPartCommand>
{
  private readonly IDraftRepository _draftsRepository = draftRepository;
  public async Task<Result> Handle(AddReleaseToDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftsRepository.GetByDraftPartIdAsync(DraftPartId.Create(request.DraftPartId), cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(draft!.Id.Value));
    }

    var draftPart = draft.Parts.FirstOrDefault(p => p.Id == DraftPartId.Create(request.DraftPartId));

    if (draftPart is null)
    {
      return Result.Failure(DraftErrors.DraftPartNotFound(request.DraftPartId));
    }

    draftPart.AddRelease(ReleaseChannel.FromName(request.ReleaseChannel), request.ReleaseDate);

    _draftsRepository.Update(draft);
    return Result.Success();
  }
}
