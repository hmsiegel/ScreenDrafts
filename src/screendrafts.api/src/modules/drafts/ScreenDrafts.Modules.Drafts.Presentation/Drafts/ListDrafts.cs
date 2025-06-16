namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class ListDrafts(ISender sender) : Endpoint<ListDraftsRequest, Result<List<DraftResponse>>>
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
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(ListDraftsRequest req, CancellationToken ct)
  {
    var query = new ListDraftsQuery(
      FromDate: req.FromDate,
      ToDate: req.ToDate,
      DraftType: req.DraftType,
      MinDrafters: req.MinDrafters,
      MaxDrafters: req.MaxDrafters,
      MinPicks: req.MinPicks,
      MaxPicks: req.MaxPicks);

    var drafts = (await _sender.Send(query, ct)).Value.ToList();

    if (drafts.Count != 0)
    {
      await SendOkAsync(Result.Success(drafts), ct);
    }
    else
    {
      await SendNoContentAsync(ct);
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
  int? MaxPicks = null);

internal sealed class ListDraftsSummary : Summary<ListDrafts>
{
  public ListDraftsSummary()
  {
    Summary = "Get all drafts";
    Description = "Get all drafts. This endpoint returns a list of all drafts in the system.";
    Response<List<DraftResponse>>(StatusCodes.Status200OK, "List of drafts.");
    Response(StatusCodes.Status204NoContent, "No drafts found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
