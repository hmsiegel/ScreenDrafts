namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.AssignSubDraftTriviaResults;

internal sealed class Endpoint : ScreenDraftsEndpoint<AssignSubDraftTriviaRequest>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.SubDraftTrivia);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
      .WithName(DraftsOpenApi.Names.SubDrafts_AssignTriviaResults)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.SubDraftUpdate);
  }

  public override async Task HandleAsync(AssignSubDraftTriviaRequest req, CancellationToken ct)
  {
    List<TriviaResultEntry> entries = [];

    foreach (var r in req.Results)
    {
      if(!ParticipantKind.TryFromValue(r.Kind, out var participantKind))
      {
        AddError(r => r.Results, "Invalid participant kind: {0}"+ r.Kind);
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

    var command = new AssignSubDraftTriviaCommand
    {
      DraftPartPublicId = req.DraftPartPublicId,
      SubDraftPublicId = req.SubDraftPublicId,
      Results = entries
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
