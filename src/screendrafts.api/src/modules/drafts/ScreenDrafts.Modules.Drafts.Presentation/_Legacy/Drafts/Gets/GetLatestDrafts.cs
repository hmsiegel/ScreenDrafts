using ScreenDrafts.Modules.Drafts.Application._Legacy.Drafts.Queries.GetDraft;

namespace ScreenDrafts.Modules.Drafts.Presentation._Legacy.Drafts.Gets;

internal sealed class GetLatestDrafts(ISender sender) : EndpointWithoutRequest<List<DraftResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/latest");
    Description(x =>
    {
      x.WithTags(_Legacy.Tags.Drafts)
      .WithDescription("Gets the last five completed drafts.")
      .WithName(nameof(GetLatestDrafts));
    });
    Policies(_Legacy.Permissions.SearchDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var canViewPatreon = User.HasClaim(c => c.Type == "permission"
      && c.Value == _Legacy.Permissions.PatronSearchDrafts);

    var query = new GetLatestDraftsQuery(IsPatreonOnly: canViewPatreon);
    var result = await _sender.Send(query, ct);
    if (result.IsFailure)
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var draftList = result.Value.ToList();
      await Send.OkAsync(draftList, ct);
    }
  }
}

internal sealed class GetLatestDraftsSummary : Summary<GetLatestDrafts>
{
  public GetLatestDraftsSummary()
  {
    Description = "Gets the last five completed drafts.";
    Summary = "Gets the last five completed drafts.";
    Response<List<DraftResponse>>(StatusCodes.Status200OK, "List of the last five completed drafts.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
  }
}
