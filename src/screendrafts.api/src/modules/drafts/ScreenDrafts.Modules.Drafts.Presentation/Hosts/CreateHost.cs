namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;

internal sealed class CreateHost(ISender sender) : Endpoint<CreateHostRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/hosts/{id:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Hosts)
      .WithName(nameof(CreateHost))
      .WithDescription("Create a new host");
    });
    AllowAnonymous();
  }
  public override async Task HandleAsync(CreateHostRequest req, CancellationToken ct)
  {
    var command = new CreateHostCommand(req.Id);
    var hostId = await _sender.Send(command, ct);
    await this.MapResultsAsync(hostId, ct);
  }
}

public sealed record CreateHostRequest(
    [FromRoute(Name = "id")] Guid Id);

internal sealed class CreateHostSummary : Summary<CreateHost>
{
  public CreateHostSummary()
  {
    Summary = "Create a new host";
    Description = "Create a new host";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the created host.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status401Unauthorized, "Unauthorized.");
  }
}
