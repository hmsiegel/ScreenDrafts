using ScreenDrafts.Modules.Drafts.Application._Legacy.Hosts.Queries.GetHost;

namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Hosts;

internal sealed class ListHosts(ISender sender) : Endpoint<ListHostsRequest, Result<PagedResult<HostResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/hosts");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Hosts)
      .WithDescription("Get all hosts")
      .WithName(nameof(ListHosts));
    });
    Policies(_Legacy.Permissions.GetHosts);
  }
  public override async Task HandleAsync(ListHostsRequest req, CancellationToken ct)
  {
    var query = new ListHostsQuery(
      Page: req.Page,
      PageSize: req.PageSize,
      Search: req.Search,
      Sort: req.Sort,
      Dir: req.Dir);

    var hosts = (await _sender.Send(query, ct)).Value;

    if (hosts.Items.Count != 0)
    {
      await Send.OkAsync(Result.Success(hosts), ct);
    }
    else
    {
      await Send.NoContentAsync(ct);
    }
  }
}

public sealed record ListHostsRequest(
  int Page,
  int PageSize,
  string? Search = null,
  string? Sort = null,
  string? Dir = null);

internal sealed class ListHostsSummary : Summary<ListHosts>
{
  public ListHostsSummary()
  {
    Description = "Get all hosts";
    Response<List<HostResponse>>(200, "List of hosts");
    Response(204, "No content");
  }
}
