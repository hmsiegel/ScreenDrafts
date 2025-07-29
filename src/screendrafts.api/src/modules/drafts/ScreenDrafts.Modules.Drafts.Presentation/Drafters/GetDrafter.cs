namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class GetDrafter(ISender sender) : Endpoint<GetDrafterRequest, DrafterResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafters/{id}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafters)
      .WithDescription("Get a drafter by ID")
      .WithName(nameof(GetDrafter));
    });
    Policies(Presentation.Permissions.GetDrafters);
  }

  public override async Task HandleAsync(GetDrafterRequest req, CancellationToken ct)
  {
    var query = new GetDrafterQuery(req.Id);
    var drafter = await _sender.Send(query, ct);
    await SendOkAsync(drafter.Value!, ct);
  }
}

public sealed record GetDrafterRequest(
    [FromRoute(Name = "id")] Guid Id);

internal sealed class GetDrafterSummary : Summary<GetDrafter>
{
  public GetDrafterSummary()
  {
    Summary = "Get a drafter by ID";
    Description = "Get a drafter by ID. This endpoint returns the details of a drafter with the specified ID.";
    Response<DrafterResponse>(StatusCodes.Status200OK, "Details of the drafter with the specified ID.");
    Response(StatusCodes.Status404NotFound, "Drafter not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource.");
  }
}
