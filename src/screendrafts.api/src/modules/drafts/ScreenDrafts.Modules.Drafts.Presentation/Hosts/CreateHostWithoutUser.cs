namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;
internal sealed class CreateHostWithoutUser(ISender sender) : Endpoint<HostRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/hosts");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Hosts)
      .WithDescription("Create a host without user")
      .WithName(nameof(CreateHostWithoutUser));
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(HostRequest req, CancellationToken ct)
  {
    var command = new CreateHostWithoutUserCommand(req.Name);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record HostRequest(string Name);

internal sealed class  CreateHostWithoutUserSummary : Summary<CreateHostWithoutUser>
{
  public CreateHostWithoutUserSummary()
  {
    Summary = "Create a host without user";
    Description = "Create a host without user";
    Response<Guid>(StatusCodes.Status200OK, "Host created successfully");
    Response<ValidationProblemDetails>(StatusCodes.Status400BadRequest, "Invalid request data");
    Response<FastEndpoints.ProblemDetails>(StatusCodes.Status500InternalServerError, "Internal server error");
  }
}
