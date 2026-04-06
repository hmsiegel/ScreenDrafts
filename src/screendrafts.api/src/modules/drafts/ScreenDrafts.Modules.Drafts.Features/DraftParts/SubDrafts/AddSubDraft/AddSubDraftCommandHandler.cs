namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AddSubDraft;

internal sealed class AddSubDraftCommandHandler(
  IDraftPartRepository draftPartRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<AddSubDraftCommand, string>
{
  private readonly IDraftPartRepository _draftPartRepository = draftPartRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(AddSubDraftCommand request, CancellationToken cancellationToken)
  {
    var draftPart = await _draftPartRepository.GetByPublicIdWithSubDraftsAsync(
      request.DraftPartPublicId,
      cancellationToken);

    if (draftPart is null)
    {
      return Result.Failure<string>(DraftPartErrors.NotFound(request.DraftPartPublicId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.SubDraft);

    var result = draftPart.AddSubDraft(request.Index, publicId);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    _draftPartRepository.Update(draftPart);

    return Result.Success(publicId);
  }
}
