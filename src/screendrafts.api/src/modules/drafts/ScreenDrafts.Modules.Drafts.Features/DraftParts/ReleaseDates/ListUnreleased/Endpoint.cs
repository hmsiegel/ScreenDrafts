namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.ReleaseDates.ListUnreleased;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListUnreleasedDraftPartsRequest, UnreleasedDraftPartResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.Unreleased);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.DraftParts_ListUnreleased)
        .Produces<PagedResult<UnreleasedDraftPartResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    // Same gate as SearchDrafts — this powers the same admin drafts page.
    Policies(DraftsAuth.Permissions.DraftList);
  }

  public override async Task HandleAsync(ListUnreleasedDraftPartsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var query = new ListUnreleasedDraftPartsQuery
    {
      Page = req.Page,
      PageSize = req.PageSize,
      DraftPublicId = req.DraftPublicId,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
