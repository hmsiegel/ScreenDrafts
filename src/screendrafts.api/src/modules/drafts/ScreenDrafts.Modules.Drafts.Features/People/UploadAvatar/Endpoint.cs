namespace ScreenDrafts.Modules.Drafts.Features.People.UploadAvatar;

internal sealed class Endpoint : ScreenDraftsEndpoint<UploadAvatarRequest, UploadAvatarResponse>
{
  public override void Configure()
  {
    Post(PeopleRoutes.Avatar);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
        .WithName(DraftsOpenApi.Names.People_UploadAvatar)
        .Produces<UploadAvatarResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(UploadAvatarRequest req, CancellationToken ct)
  {
    var files = Files[0];

    if (files is null)
    {
      AddError("No file uploaded.");
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var command = new UploadAvatarCommand
    {
      PublicId = req.PublicId,
      FileStream = files.OpenReadStream(),
      FileName = files.FileName,
      ContentType = files.ContentType,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
