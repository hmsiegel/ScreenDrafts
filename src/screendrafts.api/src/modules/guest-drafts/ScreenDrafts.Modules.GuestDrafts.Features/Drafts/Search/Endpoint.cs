namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.Search;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchGuestDraftsRequest, PagedResult<GuestDraftSummaryResponse>>
{
  public override void Configure()
  {
    Get(GuestDraftsRoutes.Search);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_Search)
        .Produces<PagedResult<GuestDraftSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftRead);
  }

  public override async Task HandleAsync(SearchGuestDraftsRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var query = new SearchDraftsQuery
    {
      CallerUserPublicId = userPublicId,
      Page = req.Page,
      PageSize = req.PageSize,
      Status = req.Status,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
