namespace ScreenDrafts.Modules.Users.Features.Users.ChangePassword;

internal sealed class Endpoint : ScreenDraftsEndpoint<ChangePasswordRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.Password);
    Description(d =>
    {
      d.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_UpdateUserPassword)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(Features.Permissions.UserUpdate);
  }

  public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
  {
    if (req.Password != req.ConfirmPassword)
    {
      AddError(r => r.ConfirmPassword, UserErrors.PasswordsDoNotMatch.Description);
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var usedrId = User.GetPublicId();
    var command = new ChangePasswordCommand
    {
      PublicId = usedrId,
      CurrentPassword = req.CurrentPassword,
      Password = req.Password,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
