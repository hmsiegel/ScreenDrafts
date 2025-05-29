namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class ListDrafts(ISender sender) : EndpointWithoutRequest<Result<List<DraftResponse>>>
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

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListDraftsQuery();

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
