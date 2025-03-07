namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;
internal sealed class AssignTriviaResults(ISender sender) : Endpoint<TriviaResultRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("drafts/{draftId:guid}/trivia/{drafterId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Trivia));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(TriviaResultRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");

    var drafterId = Route<Guid>("drafterId");

    var command = new AssignTriviaResultsCommand(
      drafterId,
      draftId,
      req.QuestionsWon,
      req.Position);

    var result = await _sender.Send(command, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(ct);
    }
  }
}

public sealed record TriviaResultRequest(int Position, int QuestionsWon);
