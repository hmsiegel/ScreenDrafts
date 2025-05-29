namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;
internal sealed class AssignTriviaResults(ISender sender) : Endpoint<TriviaResultRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("drafts/{draftId:guid}/trivia/{drafterId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Trivia)
      .WithName(nameof(AssignTriviaResults))
      .WithDescription("Assign trivia results to a drafter");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(TriviaResultRequest req, CancellationToken ct)
  {
    var command = new AssignTriviaResultsCommand(
      req.DrafterId,
      req.DraftId,
      req.QuestionsWon,
      req.Position);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record TriviaResultRequest(
  int Position,
  int QuestionsWon,
  [FromRoute(Name = "draftId")] Guid DraftId,
  [FromRoute(Name = "drafterId")] Guid DrafterId);

internal sealed class AssignTriviaResultsSummary : Summary<AssignTriviaResults>
{
  public AssignTriviaResultsSummary()
  {
    Summary = "Assign trivia results to a drafter";
    Description = "Assign trivia results to a drafter. This endpoint allows you to assign trivia results to a drafter in a draft.";
    Response(StatusCodes.Status200OK, "Trivia results assigned successfully.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
