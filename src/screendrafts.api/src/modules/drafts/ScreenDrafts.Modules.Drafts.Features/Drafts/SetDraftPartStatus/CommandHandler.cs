namespace ScreenDrafts.Modules.Drafts.Features.Drafts.SetDraftPartStatus;

internal sealed class CommandHandler(
  IDraftsRepository draftsRepository,
  IDateTimeProvider dateTimeProvider)
  : Common.Features.Abstractions.Messaging.ICommandHandler<Command, Response>
{
  private readonly IDraftsRepository _draftsRepository = draftsRepository;
  private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

  public async Task<Result<Response>> Handle(Command request, CancellationToken cancellationToken)
  {
    var req = request.Request;
    var utcNow = _dateTimeProvider.UtcNow;

    var draft = await _draftsRepository.GetDraftByPublicIdWithPartsAsync(req.DraftPublicId, cancellationToken);

    if (draft is null)
    {
      return Result<Response>.ValidationFailure(DraftErrors.NotFound(req.DraftPublicId));
    }

    var part = draft.Parts.FirstOrDefault(p => p.PartIndex == req.PartIndex);

    if (part is null)
    {
      return Result.Failure<Response>(DraftErrors.DraftPartNotFoundByIndex(req.DraftPublicId, req.PartIndex));
    }

    var result = req.Action switch
    {
      DraftPartStatusAction.Start => draft.StartPart(part.Id, utcNow),
      DraftPartStatusAction.Complete => draft.CompletePart(part.Id, utcNow),
      _ => Result.Failure<Response>(DraftErrors.InvalidDraftPartStatusAction)
    };

    if (result.IsFailure)
    {
      return Result.Failure<Response>(result.Errors[0]);
    }

    _draftsRepository.Update(draft);

    var draftLifecycle = draft.GetLifecycleView(utcNow);
    var draftPartLifecycle = part.GetLifecycleView(utcNow);

    return Result.Success(new Response
    {
      DraftPublicId = draft.PublicId,
      PartIndex = part.PartIndex,
      DraftPartId = part.Id.Value,
      DraftStatus = draft.DraftStatus.Name,
      DraftLifecylce = draftLifecycle.ToString(),
      DraftPartStatus = part.Status.ToString(),
      DraftPartLifecycle = draftPartLifecycle.ToString()
    });
  }
}
