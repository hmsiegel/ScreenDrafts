namespace ScreenDrafts.Modules.Drafts.Features.Drafts.DraftPools.Create;

internal sealed class CreateDraftPoolCommandHandler(
  IDraftRepository draftRepository,
  IPublicIdGenerator publicIdGenerator)
  : ICommandHandler<CreateDraftPoolCommand>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(CreateDraftPoolCommand request, CancellationToken cancellationToken)
  {
    var draft = await _draftRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);

    if (draft is null)
    {
      return Result.Failure(DraftErrors.NotFound(request.PublicId));
    }

    var poolResult = draft.CreatePool(_publicIdGenerator.GeneratePublicId(PublicIdPrefixes.DraftPool));

    if (poolResult.IsFailure)
    {
      return Result.Failure(poolResult.Errors);
    }

    _draftRepository.Update(draft);

    return Result.Success();
  }
}
