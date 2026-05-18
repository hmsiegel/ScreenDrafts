namespace ScreenDrafts.Modules.Drafts.Features.Participants.List;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<ListParticipantsRequest, ListParticipantsResponse>
{
  public override void Configure()
  {
    Get(ParticipantRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Participants)
        .WithName(DraftsOpenApi.Names.Participants_ListParticipants)
        .Produces<PagedResult<ParticipantListItem>>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(ListParticipantsRequest req, CancellationToken ct)
  {
    var includePatreon =
      User.Identity?.IsAuthenticated == true
      && User.HasPermission(DraftsAuth.Permissions.DraftReadPatreon);

    var query = new ListParticipantsQuery
    {
      Q = req.Q,
      Role = req.Role,
      Retired = req.Retired,
      Sort = req.Sort,
      IncludePatreon = includePatreon,
      HonorificValue = int.TryParse(req.Honorific, out var honorificValue) ? honorificValue : null,
      Page = req.Page ?? 1,
      PageSize = req.PageSize ?? 24,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
