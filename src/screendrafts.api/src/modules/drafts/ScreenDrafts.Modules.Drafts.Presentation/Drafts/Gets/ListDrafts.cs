namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Gets;

internal sealed class ListDrafts(ISender sender) : Endpoint<ListDraftsRequest, Result<PagedResult<DraftResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithDescription("Get all drafts")
      .WithName(nameof(ListDrafts));
    });
    Policies(Presentation.Permissions.SearchDrafts);
  }

  public override async Task HandleAsync(ListDraftsRequest req, CancellationToken ct)
  {
    var canViewPatreon = User.HasClaim(c => c.Type == "permission"
      && c.Value == Presentation.Permissions.PatronSearchDrafts);

    var query = new ListDraftsQuery(
      FromDate: req.FromDate,
      ToDate: req.ToDate,
      DraftType: req.DraftType,
      MinDrafters: req.MinDrafters,
      MaxDrafters: req.MaxDrafters,
      MinPicks: req.MinPicks,
      MaxPicks: req.MaxPicks,
      Sort: req.Sort,
      Dir: req.Dir,
      Q: req.Q?.Trim(),
      Page: req.Page,
      PageSize: req.PageSize,
      IsPatreonOnly: canViewPatreon);

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

public sealed record ListDraftsRequest(
  DateOnly? FromDate = null,
  DateOnly? ToDate = null,
  IEnumerable<int>? DraftType = null,
  int? MinDrafters = null,
  int? MaxDrafters = null,
  int? MinPicks = null,
  int? MaxPicks = null,
  string? Sort = null,
  string? Dir = null,
  string? Q = null,
  int Page = 1,
  int PageSize = 5);

internal sealed class ListDraftsSummary : Summary<ListDrafts>
{
  public ListDraftsSummary()
  {
    Summary = "Get all drafts";
    Description = "Get all drafts. This endpoint returns a list of all drafts in the system.";
    Response<PagedResult<DraftResponse>>(StatusCodes.Status200OK, "List of drafts.");
    Response(StatusCodes.Status204NoContent, "No drafts found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
