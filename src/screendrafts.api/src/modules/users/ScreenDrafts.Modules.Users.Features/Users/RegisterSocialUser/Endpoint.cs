namespace ScreenDrafts.Modules.Users.Features.Users.RegisterSocialUser;

internal sealed class Endpoint : ScreenDraftsEndpoint<RegisterSocialUserRequest, string>
{
  private const string SharedSecretHeader = "X-Keycloak-Secret";

  public override void Configure()
  {
    Post(UserRoutes.Social);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
      .WithName(UsersOpenApi.Names.Users_RegisterSocialUser)
      .Produces<string>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(RegisterSocialUserRequest req, CancellationToken ct)
  {
    var expectedSecret = Environment.GetEnvironmentVariable("KeycloakRegistration__Secret");
    var providedSecret = HttpContext.Request.Headers[SharedSecretHeader].FirstOrDefault();

    if (string.IsNullOrEmpty(expectedSecret) || providedSecret != expectedSecret)
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var command = new RegisterSocialUserCommand
    {
      Email = req.Email,
      FirstName = req.FirstName,
      LastName = req.LastName,
      IdentityId = req.IdentityId
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(publicId => new CreatedResponse(publicId)),
      id => UserLocations.ById(id.PublicId),
      ct);
  }
}
