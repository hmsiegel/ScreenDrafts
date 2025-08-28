namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Gets;

internal sealed class GetDraftPicks(ISender sender) : Endpoint<GetDraftPicksRequest, List<DraftPickResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{draftId:guid}/picks");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Picks)
      .WithDescription("Get all picks for a draft")
      .WithName(nameof(GetDraftPicks));
    });
    Policies(Presentation.Permissions.GetDrafts);
  }
  public override async Task HandleAsync(GetDraftPicksRequest req, CancellationToken ct)
  {
    var query = new GetDraftPicksByDraftQuery(req.DraftId);

    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await Send.OkAsync(result.Value, ct);
    }
  }
}

public sealed record GetDraftPicksRequest(
    [FromRoute(Name = "draftId")] Guid DraftId);

internal sealed class GetDraftPicksSummary : Summary<GetDraftPicks>
{
  public GetDraftPicksSummary()
  {
    Summary = "Get all picks for a draft";
    Description = "Get all picks for a draft. This includes the position, movie ID, movie title, drafter ID, and drafter name.";
    Response<List<DraftPickResponse>>(StatusCodes.Status200OK, "List of draft picks.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to get picks for this draft.");
  }
}
