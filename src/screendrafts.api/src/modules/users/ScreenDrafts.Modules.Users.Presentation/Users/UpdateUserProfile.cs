namespace ScreenDrafts.Modules.Users.Presentation.Users;

internal sealed class UpdateUserProfile : Endpoint<UpdateUserRequest>
{
  private readonly ISender _sender;
  public UpdateUserProfile(ISender sender) => _sender = sender;
  public override void Configure()
  {
    Put("/users/{userId}/profile");
    Description(x => x.WithTags(Presentation.Tags.Users));
    AllowAnonymous();
  }
  public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
  {
    var userId = Route<Guid>("userId");

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
