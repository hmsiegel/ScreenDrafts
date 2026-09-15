namespace ScreenDrafts.Modules.Users.Features.Users.EmailChange.ListCandidates;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<
    ListEmailBootstrapCandidatesRequest,
    PagedResult<EmailBootstrapCandidateItem>
  >
{
  public override void Configure()
  {
    Get(UserRoutes.EmailChangeBootstrapCandidates);
    Description(x =>
    {
      x.WithTags(UsersOpenApi.Tags.Users)
        .WithName(UsersOpenApi.Names.Users_ListEmailBootstrapCandidates)
        .Produces<PagedResult<EmailBootstrapCandidateItem>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    });
    Policies(Features.Permissions.UsersTokens);
  }

  public override async Task HandleAsync(
    ListEmailBootstrapCandidatesRequest req,
    CancellationToken ct
  )
  {
    var query = new ListEmailBootstrapCandidatesQuery
    {
      Search = req.Search,
      Page = req.Page,
      PageSize = req.PageSize,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
