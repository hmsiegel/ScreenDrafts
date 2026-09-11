namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.Search;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<SearchGuestDraftersRequest, IReadOnlyList<GuestDrafterSummaryResponse>>
{
  public override void Configure()
  {
    Get(GuestDrafterRoutes.Search);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafters)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafters_Search)
        .Produces<IReadOnlyList<GuestDrafterSummaryResponse>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status403Forbidden)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDrafterSearch);
  }

  public override async Task HandleAsync(SearchGuestDraftersRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var query = new SearchDraftersQuery { Search = req.Search };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
