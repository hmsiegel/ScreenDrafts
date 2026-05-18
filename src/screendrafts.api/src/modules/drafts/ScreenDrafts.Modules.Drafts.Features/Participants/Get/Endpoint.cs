namespace ScreenDrafts.Modules.Drafts.Features.Participants.Get;

internal sealed class Endpoint
  : ScreenDraftsEndpoint<GetParticipantProfileRequest, GetParticipantProfileResponse>
{
  public override void Configure()
  {
    Get(ParticipantRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Participants)
        .WithName(DraftsOpenApi.Names.Participants_GetById)
        .Produces<GetParticipantProfileResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetParticipantProfileRequest req, CancellationToken ct)
  {
    var includePatreon =
      User.Identity?.IsAuthenticated == true
      && User.HasPermission(DraftsAuth.Permissions.DraftReadPatreon);

    var query = new GetParticipantProfileQuery
    {
      PersonPublicId = req.PersonPublicId,
      IncludePatreon = includePatreon,
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
