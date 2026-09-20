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
