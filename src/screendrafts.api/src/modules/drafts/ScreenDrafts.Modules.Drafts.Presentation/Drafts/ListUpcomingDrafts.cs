namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class ListUpcomingDrafts(ISender sender) : EndpointWithoutRequest<List<DraftResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/upcoming");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(ListUpcomingDrafts))
      .WithDescription("Get all upcoming drafts");
    });

    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListUpcomingDraftsQuery();
    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var draftList = result.Value.ToList();
      await SendOkAsync(draftList, ct);
    }
  }
}

internal sealed class ListUpcomingDraftsSummary : Summary<ListUpcomingDrafts>
{
  public ListUpcomingDraftsSummary()
  {
    Summary = "Get all upcoming drafts";
    Description = "Get all upcoming drafts. This endpoint returns a list of all upcoming drafts.";
    Response<List<DraftResponse>>(StatusCodes.Status200OK, "List of all upcoming drafts.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
