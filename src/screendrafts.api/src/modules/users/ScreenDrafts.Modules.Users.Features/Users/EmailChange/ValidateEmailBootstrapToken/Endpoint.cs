namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ValidateEmailBootstrapToken;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ValidateEmailBootstrapTokenRequest, EmailBootstrapTokenValidationResponse>
{
  public override void Configure()
  {
    Get(UserRoutes.EmailChangeBootstrapValidate);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_ValidateEmailBootstrapToken)
        .Produces<EmailBootstrapTokenValidationResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(
    ValidateEmailBootstrapTokenRequest req,
    CancellationToken ct
  )
  {
    var query = new ValidateEmailBootstrapTokenQuery { Token = req.Token };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
