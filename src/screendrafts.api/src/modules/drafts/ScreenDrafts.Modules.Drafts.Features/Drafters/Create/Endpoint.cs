using ScreenDrafts.Common.Abstractions.Results;

namespace ScreenDrafts.Modules.Drafts.Features.Drafters.Create;

internal sealed class Endpoint : ScreenDraftsEndpoint<CreateDrafterRequest, string>
{
  public override void Configure()
  {
    Post(DrafterRoutes.Drafters);
    Description(x =>
    {
      x.WithName(DraftsOpenApi.Names.Drafters_CreateDrafter)
      .WithTags(DraftsOpenApi.Tags.Drafters)
      .Produces<string>(StatusCodes.Status201Created)
      .Produces(StatusCodes.Status400BadRequest)
      .Produces(StatusCodes.Status401Unauthorized)
      .Produces(StatusCodes.Status403Forbidden)
      .Produces(StatusCodes.Status409Conflict);
    });
    Permissions(DraftsAuth.Permissions.DrafterCreate);
  }

  public override async Task HandleAsync(CreateDrafterRequest req, CancellationToken ct)
  {
    var CreateDrafterCommand = new CreateDrafterCommand(req.PersonId);

    var result = await Sender.Send(CreateDrafterCommand, ct);

    await this.SendCreatedAsync(
      result.Map(id => new CreatedResponse(id)),
      created => DrafterLocations.ById(created.PublicId),
      ct);
  }
}


