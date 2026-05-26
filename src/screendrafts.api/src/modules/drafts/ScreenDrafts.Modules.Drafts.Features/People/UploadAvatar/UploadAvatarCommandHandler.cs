namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed class UploadAvatarCommandHandler(
  IPersonRepository personRepository,
  IWebHostEnvironment webHostEnvironment
) : ICommandHandler<UploadAvatarCommand, UploadAvatarResponse>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

  private static readonly string[] _allowedExtensions = ["image/jpeg", "image/png", "image/webp"];

  public async Task<Result<UploadAvatarResponse>> Handle(
    UploadAvatarCommand request,
    CancellationToken cancellationToken
  )
  {
    if (!_allowedExtensions.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
    {
      return Result.Failure<UploadAvatarResponse>(PersonErrors.InvalidAvatarContentType);
    }

    var person = await _personRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
    if (person is null)
    {
      return Result.Failure<UploadAvatarResponse>(PersonErrors.NotFound(request.PublicId));
    }

    var ext = request.ContentType switch
    {
      "image/jpeg" => "jpg",
      "image/png" => "png",
      "image/webp" => "webp",
      _ => throw new InvalidOperationException("Unsupported content type."),
    };

    var fileName = $"{request.PublicId}.{ext}";
    var physialDir = Path.Combine(_webHostEnvironment.WebRootPath, "drafters");

    Directory.CreateDirectory(physialDir);

    var physicalPath = Path.Combine(physialDir, fileName);
    await using var fileStream = File.Create(physicalPath);
    await request.FileStream.CopyToAsync(fileStream, cancellationToken);

    person.UpdateProfilePicture(fileName);
    _personRepository.Update(person);

    return Result.Success(new UploadAvatarResponse { AvatarPath = fileName });
  }
}
