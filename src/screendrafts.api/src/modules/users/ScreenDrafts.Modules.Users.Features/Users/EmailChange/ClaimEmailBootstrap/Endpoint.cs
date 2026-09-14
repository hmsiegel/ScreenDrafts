namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ClaimEmailBootstrap;

internal sealed class Endpoint : ScreenDraftsEndpoint<ClaimEmailBootstrapRequest>
{
  public override void Configure()
  {
    Post(UserRoutes.EmailChangeBootstrapClaim);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_ClaimEmailBootstrapToken)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ClaimEmailBootstrapRequest req, CancellationToken ct)
  {
    var command = new ClaimEmailBootstrapCommand { Token = req.Token, NewEmail = req.NewEmail };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
