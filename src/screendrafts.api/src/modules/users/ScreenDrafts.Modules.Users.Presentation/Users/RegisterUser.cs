namespace ScreenDrafts.Modules.Users.Presentation.Users;

internal sealed class RegisterUser(ISender sender) : Endpoint<RegisterUserRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/users/register");
    Description(x => x.WithTags(Presentation.Tags.Users));
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
  {
    var command = new RegisterUserCommand(
      req.Email,
      req.Password,
      req.FirstName,
      req.LastName,
      req.MiddleName);

    var result = await _sender.Send(command, ct);

    await SendOkAsync(result.Value, ct);
  }
}

public sealed record RegisterUserRequest(
  string Email,
  string Password,
  string FirstName,
  string LastName,
  string? MiddleName = null);
