namespace ScreenDrafts.Modules.Administration.Features.Users.SendPasswordReset;

internal sealed class Endpoint : ScreenDraftsEndpoint<SendPasswordResetRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.UserPasswordReset);
    Description(x =>
    {
      x.WithTags(AdministrationOpenApi.Tags.Administration)
        .WithName(AdministrationOpenApi.Names.Administration_SendPasswordReset)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(AdministrationAuth.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(SendPasswordResetRequest req, CancellationToken ct)
  {
    var command = new SendPasswordResetCommand { PublicId = req.PublicId };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
