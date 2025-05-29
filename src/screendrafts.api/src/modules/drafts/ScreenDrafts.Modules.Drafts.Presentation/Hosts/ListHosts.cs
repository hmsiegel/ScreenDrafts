namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;

internal sealed class ListHosts(ISender sender) : EndpointWithoutRequest<Result<List<HostResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/hosts");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Hosts)
      .WithDescription("Get all hosts")
      .WithName(nameof(ListHosts));
    });
    Policies(Presentation.Permissions.GetHosts);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListHostsQuery();

    var drafters = (await _sender.Send(query, ct)).Value.ToList();

    if (drafters.Count != 0)
    {
      await SendOkAsync(Result.Success(drafters), ct);
    }
    else
    {
      await SendNoContentAsync(ct);
    }
  }
}

internal sealed class ListHostsSummary : Summary<ListHosts>
{
  public ListHostsSummary()
  {
    Description = "Get all hosts";
    Response<List<HostResponse>>(200, "List of hosts");
    Response(204, "No content");
  }
}
