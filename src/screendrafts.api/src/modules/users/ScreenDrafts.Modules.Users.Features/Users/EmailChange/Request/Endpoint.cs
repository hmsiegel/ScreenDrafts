namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Request;

internal sealed class Endpoint : ScreenDraftsEndpoint<RequestEmailChangeRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.EmailChangeRequest);
    Description(d =>
    {
      d.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_RequestEmailChange)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);
    });
    Policies(Features.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(RequestEmailChangeRequest req, CancellationToken ct)
  {
    var publicId = User.GetUserPublicId();

    var command = new RequestEmailChangeCommand { PublicId = publicId, NewEmail = req.NewEmail };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
