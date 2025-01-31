namespace ScreenDrafts.Modules.Users.Presentation.Users;
internal sealed class GetUserProfile(ISender sender) : EndpointWithoutRequest<UserResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/users/profile");
    Description(x => x.WithTags(Presentation.Tags.Users));
    Policies( Presentation.Permissions.GetUser);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = User.GetUserId();

    var query = new GetUserQuery(userId);

    var result = await _sender.Send(query, ct);

    await SendOkAsync(result.Value, ct);
  }
}
