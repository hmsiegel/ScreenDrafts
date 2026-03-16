namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.AddCandidateListEntry;

internal sealed class Endpoint : ScreenDraftsEndpoint<AddCandidateEntryRequest, AddCanidateEntryResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.CandidateList);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.CandidateLists)
      .WithName(DraftsOpenApi.Names.CandidateLists_AddEntry)
      .Produces<AddCanidateEntryResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CandidateListCreate);
  }

  public override async Task HandleAsync(AddCandidateEntryRequest req, CancellationToken ct)
  {
    var actorPublicId = User.GetPublicId() ?? string.Empty;

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    var command = new AddCandidateEntryCommand
    {
      DraftPartId = req.DraftPartId,
      TmdbId = req.TmdbId,
      Notes = req.Notes,
      AddedByPublicId = actorPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
