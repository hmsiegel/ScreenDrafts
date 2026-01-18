namespace ScreenDrafts.Modules.Users.Features.Users.Get;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<Response>
{
  public override void Configure()
  {
    Get(UserRoutes.Profile);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
      .WithName(UsersOpenApi.Names.Users_GetUserProfile)
      .Produces<Response>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(Features.Permissions.UserRead);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var publicId = User.GetPublicId();

    var query = new Query(publicId);

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
