namespace ScreenDrafts.Modules.Users.Features.Users.Register;

internal sealed class Endpoint : ScreenDraftsEndpoint<RegisterUserRequest, string>
{
  public override void Configure()
  {
    Post(UserRoutes.Register);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
      .WithName(UsersOpenApi.Names.Users_RegisterUser)
      .Produces<string>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
  {
    var command = new RegisterUserCommand
    {
      Email = req.Email,
      Password = req.Password,
      FirstName = req.FirstName,
      LastName = req.LastName,
      MiddleName = req.MiddleName
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedIdResponse(id)),
      created => UserLocations.ById(created.Id),
      ct);
  }
}
