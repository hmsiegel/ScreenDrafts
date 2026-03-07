namespace ScreenDrafts.Modules.Drafts.Features.Drafters.DrafterProfile;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetDrafterProfileRequest, GetDrafterProfileResponse>
{
  public override void Configure()
  {
    Get(DrafterRoutes.ById);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafters)
      .WithName(DraftsOpenApi.Names.Drafters_GetDrafterById)
      .Produces<GetDrafterProfileResponse>(StatusCodes.Status200OK)
      .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.DrafterRead);
  }

  public override async Task HandleAsync(GetDrafterProfileRequest req, CancellationToken ct)
  {
    var query = new GetDrafterProfileQuery
    {
      PublicId = req.PublicId
    };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
