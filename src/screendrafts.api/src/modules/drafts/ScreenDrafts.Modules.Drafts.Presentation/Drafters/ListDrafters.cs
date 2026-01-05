namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ListDrafters(ISender sender) : Endpoint<ListDraftersRequest, Result<PagedResult<DrafterResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafters");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafters)
      .WithDescription("Get all drafters")
      .WithName(nameof(ListDrafters));
    });
    Policies(Presentation.Permissions.GetDrafters);
  }
  public override async Task HandleAsync(ListDraftersRequest req, CancellationToken ct)
  {
    var query = new ListDraftersQuery(
      Page: req.Page,
      PageSize: req.PageSize,
      Search: req.Search,
      Sort: req.Sort,
      Dir: req.Sort);

    var result = await _sender.Send(query, ct);

    if (result.IsSuccess && result.Value.Items.Count != 0)
    {
      await Send.OkAsync(result, ct);
    }
    else
    {
      await Send.NoContentAsync(ct);
    }
  }
}

public sealed record ListDraftersRequest(
  int Page = 1,
  int PageSize = 25,
  string? Search = null,
  string? Sort = null,
  string? Dir = null);

internal sealed class ListDrafterrSummary : Summary<ListDrafters>
{
  public ListDrafterrSummary()
  {
    Summary = "Get all drafters";
    Description = "Get all drafters";
    Response<List<DrafterResponse>>(200, "List of drafters");
    Response(204, "No content");
  }
}
