namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed class UploadAvatarCommandHandler(
  IPersonRepository personRepository,
  IFileStorage fileStorage
) : ICommandHandler<UploadAvatarCommand, UploadAvatarResponse>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IFileStorage _fileStorage = fileStorage;

  public async Task<Result<UploadAvatarResponse>> Handle(
    UploadAvatarCommand request,
    CancellationToken cancellationToken
  )
  {
    var ext = ImageUpload.GetExtension(request.ContentType);
    if (ext is null)
    {
      return Result.Failure<UploadAvatarResponse>(PersonErrors.InvalidAvatarContentType);
    }

    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
    if (person is null)
    {
      return Result.Failure<UploadAvatarResponse>(PersonErrors.NotFound(request.PublicId));
    }

    var fileName = ImageUpload.BuildFileName(request.PublicId, ext);

    await _fileStorage.UploadAsync(
      $"{ImageUpload.DraftersFolder}/{fileName}",
      request.FileStream,
      request.ContentType,
      ImageUpload.CacheControl,
      cancellationToken
    );

    person.UpdateProfilePicture(fileName);
    _personRepository.Update(person);

    return Result.Success(new UploadAvatarResponse { AvatarPath = fileName });
  }
}
