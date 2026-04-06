namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.SetSubDraftSubject;

internal sealed class SetSubDraftSubjectCommandHandler(IDraftPartRepository draftPartRepository)
  : ICommandHandler<SetSubDraftSubjectCommand>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;

  public async Task<Result> Handle(SetSubDraftSubjectCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var subDraft = draftPart.SubDrafts.FirstOrDefault(x => x.PublicId == request.SubDraftPublicId);
    if (subDraft is null)
    {
      return Result.Failure(SubDraftErrors.NotFound(request.SubDraftPublicId));
    }

    var subjectKind = SubjectKind.FromValue(request.SubjectKind);

    var result = subDraft.SetSubject(subjectKind, request.SubjectName);

    if (result.IsFailure)
    {
      return Result.Failure(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success();
  }
}
