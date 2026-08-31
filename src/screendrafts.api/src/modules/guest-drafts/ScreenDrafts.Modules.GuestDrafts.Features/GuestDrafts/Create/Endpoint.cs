namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateGuestDraftRequest, CreatedResponse>
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.GuestDrafts);
    Description(x =>
    {
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
      .WithName(GuestDraftsOpenApi.Names.GuestDrafts_CreateGuestDraft)
      .Produces<CreatedResponse>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status403Forbidden);
    });
    Policies(GuestDraftsAuth.Permissions.GuestDraftCreate);
  }

  public override async Task HandleAsync(CreateGuestDraftRequest req, CancellationToken ct)
  {
    ArgumentNullException.ThrowIfNull(req);

    var userPublicId = User.GetUserPublicId();

    var command = new CreateGuestDraftCommand
    {
      OwnerUserPublicId = userPublicId,
      Title = req.Title,
      Type = req.Type
    };

    var result = await Sender.Send(command, ct);

    await this.SendCreatedAsync(
      result.Map(publicId => new CreatedResponse(publicId)),
      created => GuestDraftLocations.ById(created.PublicId),
      ct);
  }
}
