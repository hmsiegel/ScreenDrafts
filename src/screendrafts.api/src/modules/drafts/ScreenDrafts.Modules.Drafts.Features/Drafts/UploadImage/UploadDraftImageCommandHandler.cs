namespace ScreenDrafts.Modules.Drafts.Features.Drafts.UploadImage;

internal sealed class UploadDraftImageCommandHandler(
  IDraftRepository draftRepository,
  IFileStorage fileStorage
) : ICommandHandler<UploadDraftImageCommand, UploadDraftImageResponse>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IFileStorage _fileStorage = fileStorage;

  public async Task<Result<UploadDraftImageResponse>> Handle(
    UploadDraftImageCommand request,
    CancellationToken cancellationToken
  )
  {
    var ext = ImageUpload.GetExtension(request.ContentType);
    if (ext is null)
    {
      return Result.Failure<UploadDraftImageResponse>(DraftErrors.InvalidImageContentType);
    }

    var draft = await _draftRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure<UploadDraftImageResponse>(DraftErrors.NotFound(request.PublicId));
    }

    var fileName = ImageUpload.BuildFileName(request.PublicId, ext);

    await _fileStorage.UploadAsync(
      $"{ImageUpload.DraftsFolder}/{fileName}",
      request.FileStream,
      request.ContentType,
      ImageUpload.CacheControl,
      cancellationToken
    );

    draft.SetImagePath(fileName);
    _draftRepository.Update(draft);

    return Result.Success(new UploadDraftImageResponse { ImagePath = fileName });
  }
}
