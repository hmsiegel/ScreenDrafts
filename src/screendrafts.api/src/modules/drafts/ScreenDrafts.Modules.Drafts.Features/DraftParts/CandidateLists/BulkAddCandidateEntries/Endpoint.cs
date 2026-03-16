namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.CandidateLists.BulkAddCandidateEntries;

internal sealed class Endpoint : ScreenDraftsEndpoint<BulkAddCandidateEntriesRequest, BulkAddMoviesResponse>
{
  public override void Configure()
  {
    Post(DraftPartRoutes.CandidateListBulkAdd);
    AllowFileUploads();
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.CandidateLists)
      .WithName(DraftsOpenApi.Names.CandidateLists_BulkAddEntries)
      .Accepts<BulkAddCandidateEntriesRequest>("multipart/form-data")
      .Produces<BulkAddMoviesResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.CandidateListUpdate);
  }

  public override async Task HandleAsync(BulkAddCandidateEntriesRequest req, CancellationToken ct)
  {
    var actorPublicId = User.GetPublicId()!;

    if (string.IsNullOrWhiteSpace(actorPublicId))
    {
      await Send.UnauthorizedAsync(ct);
      return;
    }

    using var stream = req.File.OpenReadStream();

    var command = new BulkAddCandidateEntriesCommand
    {
      DraftPartId = req.DraftPart,
      CsvStream = stream,
      AddedByPublicId = actorPublicId
    };

    var result = await Sender.Send(command, ct);

    await this.SendOkAsync(result, ct);
  }
}
