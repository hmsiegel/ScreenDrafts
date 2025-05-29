namespace ScreenDrafts.Modules.Users.Presentation.Users;

internal sealed class UpdateUserProfile(ISender sender) : Endpoint<UpdateUserRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Put("/users/profile");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Users)
      .WithName(nameof(UpdateUserProfile))
      .WithDescription("Updates the current user's profile information.");
    });
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

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record UpdateUserRequest(
  string FirstName,
  string LastName,
  string? MiddleName = null);

internal sealed class UpdateUserProfileSummary : Summary<UpdateUserProfile>
{
  public UpdateUserProfileSummary()
  {
    Summary = "Updates the current user's profile information.";
    Description = "Updates the current user's profile information.";
    Response(StatusCodes.Status200OK, "The user profile was updated successfully.");
    Response(StatusCodes.Status400BadRequest, "The request was invalid.");
    Response(StatusCodes.Status404NotFound, "The user was not found.");
  }
}
