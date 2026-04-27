namespace ScreenDrafts.Modules.Drafts.Features.Drafts.ListLatestDrafts;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<ListLatestDraftsResponse>
{
  public override void Configure()
  {
    Get(DraftRoutes.Latest);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
      .WithName(DraftsOpenApi.Names.Drafts_ListLatestDrafts)
      .Produces<ListLatestDraftsResponse>(StatusCodes.Status200OK);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var includePatreon = User.HasPermission(DraftsAuth.Permissions.DraftReadPatreon);

    var query = new ListLatestDraftsQuery
    {
      IncludePatreonOnly = includePatreon
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
