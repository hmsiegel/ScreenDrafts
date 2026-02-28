namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraftPart;

internal sealed class CreatedDraftPartCommandHandler(
  IDraftRepository draftRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreateDraftPartCommand, string>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(CreateDraftPartCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.DraftPublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure<string>(DraftErrors.NotFound(request.DraftPublicId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPart);

    var addPartResult = draft.AddPart(
      request.PartIndex,
      request.MinimumPosition,
      request.MaximumPosition,
      publicId);

    if (addPartResult.IsFailure)
    {
      return Result.Failure<string>(addPartResult.Error!);
    }

    _draftRepository.Update(draft);

    return publicId;
  }
}
