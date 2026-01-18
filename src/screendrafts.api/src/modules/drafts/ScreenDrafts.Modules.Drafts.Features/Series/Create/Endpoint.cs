namespace ScreenDrafts.Modules.Drafts.Features.Series.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<Request, CreatedIdResponse>
{
  public override void Configure()
  {
    Post(SeriesRoutes.Series);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Series)
      .WithName(DraftsOpenApi.Names.Series_CreateSeries)
      .Produces<CreatedIdResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Permissions(Features.Permissions.SeriesCreate);
  }

  public override async Task HandleAsync(Request req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);
    var command = new Command
    {
      Name = req.Name,
      Kind = req.Kind,
      CanonicalPolicy = req.CanonicalPolicy,
      ContinuityScope = req.ContinuityScope,
      ContinuityDateRule = req.ContinuityDateRule,
      AllowedDraftTypes = (int)req.AllowedDraftTypes,
      DefaultDraftType = req.DefaultDraftType
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedIdResponse(id)),
      created => SeriesLocations.ById(created.Id),
      ct);
  }
}
