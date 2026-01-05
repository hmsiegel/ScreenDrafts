namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class GetTriviaResults(ISender sender) : Endpoint<TriviaResultsRequest, TriviaResultDto>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("drafts/{draftId:guid}/trivia/{drafterId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Trivia)
      .WithName(nameof(GetTriviaResults))
      .WithDescription("Get trivia results for a drafter in a draft.");
    });
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(TriviaResultsRequest req, CancellationToken ct)
  {
    var query = new GetTriviaResultsForDrafterQuery(req.DraftId, req.DrafterId);
    var result = await _sender.Send(query, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record TriviaResultsRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "drafterId")] Guid DrafterId);

internal sealed class GetTriviaResultsSummary : Summary<GetTriviaResults>
{
  public GetTriviaResultsSummary()
  {
    Summary = "Get trivia results for a drafter in a draft.";
    Description = "Get trivia results for a drafter in a draft.";
    Response<TriviaResultDto>(200, "The trivia results for the drafter.");
    Response(400, "Invalid request.");
    Response(404, "Draft not found.");
    Response(403, "Forbidden.");
  }
}
