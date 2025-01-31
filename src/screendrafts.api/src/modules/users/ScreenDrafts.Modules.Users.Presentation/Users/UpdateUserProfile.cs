namespace ScreenDrafts.Modules.Users.Presentation.Users;

internal sealed class UpdateUserProfile(ISender sender) : Endpoint<UpdateUserRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Put("/users/profile");
    Description(x => x.WithTags(Presentation.Tags.Users));
    Policies(Presentation.Permissions.ModifyUser);
  }
  public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
  {
    var userId = User.GetUserId();

    var command = new UpdateUserCommand(
      userId,
      req.FirstName,
      req.LastName,
      req.MiddleName);

    await _sender.Send(command, ct);

    await SendOkAsync(ct);
  }
}

public sealed record UpdateUserRequest(
  string FirstName,
  string LastName,
  string? MiddleName = null);
