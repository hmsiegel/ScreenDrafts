namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;

internal sealed class GetHost(ISender sender) : EndpointWithoutRequest<HostResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/hosts/{id}");
    Description(x => x.WithTags(Presentation.Tags.Hosts));
    Policies(Presentation.Permissions.GetHosts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var id = Route<Guid>("id");
    var query = new GetHostQuery(id);
    var drafter = await _sender.Send(query, ct);
    await SendOkAsync(drafter.Value!, ct);
  }
}

