namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.GenerateEmailBootstrapTokens;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GenerateEmailBootstrapTokensRequest, List<EmailBootstrapTokenResponse>>
{
  public override void Configure()
  {
    Post(UserRoutes.EmailChangeBootstrapGenerate);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_GenerateEmailBootstrapTokens)
        .Produces<List<EmailBootstrapTokenResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    });
    Policies(Features.Permissions.UsersTokens);
  }

  public override async Task HandleAsync(
    GenerateEmailBootstrapTokensRequest req,
    CancellationToken ct
  )
  {
    var command = new GenerateEmailBootstrapTokensCommand
    {
      UserPublicIds = req.UserPublicIds,
      BatchLabel = req.BatchLabel,
      ExpiryHours = req.ExpiryHours,
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
