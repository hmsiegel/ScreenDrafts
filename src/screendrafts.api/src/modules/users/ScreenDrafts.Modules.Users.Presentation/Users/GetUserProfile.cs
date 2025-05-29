namespace ScreenDrafts.Modules.Users.Presentation.Users;

internal sealed class GetUserProfile(ISender sender) : EndpointWithoutRequest<UserResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/users/profile");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Users)
      .WithName(nameof(GetUserProfile))
      .WithDescription("Get user profile");
    });
    Policies(Presentation.Permissions.GetUser);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = User.GetUserId();

    var query = new GetUserQuery(userId);

    var result = await _sender.Send(query, ct);

    await this.MapResultsAsync(result, ct);
  }
}

internal sealed class GetUserProfileSummary : Summary<GetUserProfile>
{
  public GetUserProfileSummary()
  {
    Description = "Get user profile";
    Response<UserResponse>(StatusCodes.Status200OK, "User profile retrieved successfully.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized");
    Response(StatusCodes.Status403Forbidden, "Forbidden");
  }
}
