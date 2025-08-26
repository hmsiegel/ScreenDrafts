using ScreenDrafts.Modules.Drafts.Application.Hosts.Queries.GetHost;

namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;

internal sealed class GetHost(ISender sender) : Endpoint<GetHostRequest, HostResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/hosts/{id}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Hosts)
      .WithName(nameof(GetHost))
      .WithDescription("Get a host by ID");
    });
    Policies(Presentation.Permissions.GetHosts);
  }

  public override async Task HandleAsync(GetHostRequest req, CancellationToken ct)
  {
    var query = new GetHostQuery(req.Id);
    var drafter = await _sender.Send(query, ct);
    await Send.OkAsync(drafter.Value!, ct);
  }
}

public sealed record GetHostRequest([FromRoute(Name = "id")] Guid Id);

internal sealed class GetHostSummary : Summary<GetHost>
{
  public GetHostSummary()
  {
    Description = "Get a host by ID";
    Response<HostResponse>(StatusCodes.Status200OK, "The host was found.");
    Response(StatusCodes.Status404NotFound, "The host was not found.");
  }
}
