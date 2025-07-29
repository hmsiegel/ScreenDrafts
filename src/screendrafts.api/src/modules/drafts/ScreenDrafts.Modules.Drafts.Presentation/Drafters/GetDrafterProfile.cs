namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class GetDrafterProfile(ISender sender)
  : Endpoint<GetDrafterProfileRequest, DrafterProfileResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafters/{drafterId}/profile");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafters)
       .WithDescription("Get a drafter's profile by ID")
       .WithName(nameof(GetDrafterProfile));
    });
    Policies(Presentation.Permissions.GetDrafters);
  }

  public override async Task HandleAsync(GetDrafterProfileRequest req, CancellationToken ct)
  {
    var query = new GetDrafterProfileQuery(req.DrafterId);
    var result = await _sender.Send(query, ct);

    await this.MapResultsAsync(result, ct);
  }
}

public sealed record GetDrafterProfileRequest([FromRoute(Name = "drafterId")] Guid DrafterId);

internal sealed class GetDrafterProfileSummary : Summary<GetDrafterProfile>
{
  public GetDrafterProfileSummary()
  {
    Summary = "Get a drafter's profile by ID";
    Description = "Get a drafter's profile by ID. This endpoint returns the profile details of a drafter with the specified ID.";
    Response<DrafterProfileResponse>(StatusCodes.Status200OK, "Details of the drafter's profile with the specified ID.");
    Response(StatusCodes.Status404NotFound, "Drafter not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
