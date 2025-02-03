namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;

internal sealed class CreateHost(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/hosts/{id:guid}");
    Description(x => x.WithTags(Presentation.Tags.Hosts));
    AllowAnonymous();
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = Route<Guid>("id");


    var command = new CreateHostCommand(userId);

    var hostId = await _sender.Send(command, ct);

    await SendOkAsync(hostId.Value, ct);
  }
}

