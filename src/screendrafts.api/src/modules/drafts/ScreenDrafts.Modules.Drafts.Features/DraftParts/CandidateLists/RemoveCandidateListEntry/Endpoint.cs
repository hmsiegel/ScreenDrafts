namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.RemoveCandidateListEntry;

internal sealed class Endpoint : ScreenDraftsEndpoint<RemoveCandidateListEntryRequest>
{
  public override void Configure()
  {
    Delete(DraftPartRoutes.CandidateListEntry);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.CandidateLists)
      .WithName(DraftsOpenApi.Names.CandidateLists_RemoveEntry)
      .Produces(StatusCodes.Status204NoContent)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Permissions(DraftsAuth.Permissions.CandidateListDelete);
  }

  public override async Task HandleAsync(RemoveCandidateListEntryRequest req, CancellationToken ct)
  {
    var command = new RemoveCandidateListEntryCommand
    {
      DraftPartId = req.DraftPartId,
      TmdbId = req.TmdbId
    };

    var result = await Sender.Send(command, ct);
    await this.SendNoContentAsync(result, ct);
  }
}
