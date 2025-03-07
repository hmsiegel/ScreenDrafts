namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class GetTriviaResults(ISender sender) : EndpointWithoutRequest<TriviaResultDto>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("drafts/{draftId:guid}/trivia/{drafterId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Trivia));
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var drafterId = Route<Guid>("drafterId");

    var query = new GetTriviaResultsForDrafterQuery(drafterId, draftId);
    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(result.Value, ct);
    }
  }
}
