namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.AssignTriviaResults;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignTriviaResultsRequest>
{
  public override void Configure()
  {
    Put(DraftPartRoutes.AssignTriviaResults);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.DraftParts_AssignTriviaResults)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DraftPartUpdate);
  }

  public override async Task HandleAsync(AssignTriviaResultsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    List<TriviaResultEntry> entries = [];

    foreach (var r in req.Results)
    {
      if (!ParticipantKind.TryFromValue(r.Kind, out var participantKind))
      {
        AddError(r => r.Results, "Invalid participant kind: " + r.Kind);
        await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
        return;
      }

      entries.Add(new TriviaResultEntry
      {
        ParticipantPublicId = r.ParticipantPublicId,
        Kind = participantKind,
        Position = r.Position,
        QuestionsWon = r.QuestionsWon
      });
    }

    var command = new AssignTriviaResultsCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      Results = entries
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
