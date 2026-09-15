namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.Confirm;

internal sealed class Endpoint : ScreenDraftsEndpoint<ConfirmEmailChangeRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.EmailChangeConfirm);
    Description(d =>
    {
      d.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_ConfirmEmailChange)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ConfirmEmailChangeRequest req, CancellationToken ct)
  {
    var command = new ConfirmEmailChangeCommand { Token = req.Token };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
