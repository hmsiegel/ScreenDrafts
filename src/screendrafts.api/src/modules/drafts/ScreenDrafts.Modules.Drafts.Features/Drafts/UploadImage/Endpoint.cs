namespace ScreenDrafts.Modules.Drafts.Features.Drafts.UploadImage;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<UploadDraftImageRequest, UploadDraftImageResponse>
{
  public override void Configure()
  {
    Post(DraftRoutes.Image);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_UploadDraftImage)
        .Produces<UploadDraftImageResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftUpdate);
    AllowFileUploads();
  }

  public override async Task HandleAsync(UploadDraftImageRequest req, CancellationToken ct)
  {
    var file = Files[0];

    if (file is null)
    {
      AddError("No file uploaded.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new UploadDraftImageCommand
    {
      PublicId = req.PublicId,
      FileStream = file.OpenReadStream(),
      FileName = file.FileName,
      ContentType = file.ContentType,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}

internal sealed record UploadDraftImageCommand : ICommand<UploadDraftImageResponse>
{
  public required string PublicId { get; set; }
  public required Stream FileStream { get; set; }
  public required string FileName { get; set; }
  public required string ContentType { get; set; }
}

internal sealed record UploadDraftImageRequest
{
  [FromRoute(Name = "publicId")]
  public string PublicId { get; set; } = default!;
}

internal sealed record UploadDraftImageResponse
{
  public required string ImagePath { get; set; }
}

internal sealed class UploadDraftImageCommandHandler(
  IDraftRepository draftRepository,
  IWebHostEnvironment webHostEnvironment
) : ICommandHandler<UploadDraftImageCommand, UploadDraftImageResponse>
{
  private readonly IDraftRepository _draftRepository = draftRepository;
  private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

  private static readonly string[] _allowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

  public async Task<Result<UploadDraftImageResponse>> Handle(
    UploadDraftImageCommand request,
    CancellationToken cancellationToken
  )
  {
    if (!_allowedContentTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
    {
      return Result.Failure<UploadDraftImageResponse>(DraftErrors.InvalidImageContentType);
    }

    var draft = await _draftRepository.GetByPublicIdAsync(request.PublicId, cancellationToken);
    if (draft is null)
    {
      return Result.Failure<UploadDraftImageResponse>(DraftErrors.NotFound(request.PublicId));
    }

    var ext = request.ContentType switch
    {
      "image/jpeg" => "jpg",
      "image/png" => "png",
      "image/webp" => "webp",
      _ => throw new InvalidOperationException("Unsupported content type."),
    };

    var fileName = $"{request.PublicId}.{ext}";
    var physicalDir = Path.Combine(_webHostEnvironment.WebRootPath, "drafts");

    Directory.CreateDirectory(physicalDir);

    // Remove any existing image for this draft (different extension)
    foreach (var existingExt in new[] { "jpg", "png", "webp" })
    {
      var existing = Path.Combine(physicalDir, $"{request.PublicId}.{existingExt}");
      if (File.Exists(existing) && existingExt != ext)
      {
        File.Delete(existing);
      }
    }

    var physicalPath = Path.Combine(physicalDir, fileName);
    await using var fileStream = File.Create(physicalPath);
    await request.FileStream.CopyToAsync(fileStream, cancellationToken);

    draft.SetImagePath(fileName);
    _draftRepository.Update(draft);

    return Result.Success(new UploadDraftImageResponse { ImagePath = fileName });
  }
}
