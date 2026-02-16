namespace ScreenDrafts.Modules.Drafts.Features.Drafts.CreateDraft;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDraftRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(DraftRoutes.Base);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.Drafts)
        .WithName(DraftsOpenApi.Names.Drafts_CreateDraft)
        .Produces<CreatedResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(DraftsAuth.Permissions.DraftCreate);
  }

  public override async Task HandleAsync(CreateDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var CreateDraftCommand = new CreateDraftCommand
    {
      DraftType = req.DraftType,
      Title = req.Title,
      SeriesId = req.SeriesId,
      MinPosition = req.MinPosition,
      MaxPosition = req.MaxPosition,
      AutoCreateFirstPart = req.AutoCreateFirstPart
    };

    var result = await Sender.Send(CreateDraftCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DraftLocations.ById(created.PublicId),
      ct);
  }
}


