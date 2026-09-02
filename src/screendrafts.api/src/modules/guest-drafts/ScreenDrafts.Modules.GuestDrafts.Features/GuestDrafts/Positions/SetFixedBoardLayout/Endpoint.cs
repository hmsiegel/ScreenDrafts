using ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.SetFixedBoardLayout;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafts.Positions.SetFixedBoardLayout;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest
{
  public override void Configure()
  {
    Post(GuestDraftsRoutes.FixedBoardLayout);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.GuestDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_SetFixedBoardLayout)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound)
    );
    Policies(GuestDraftsAuth.Permissions.GuestDraftSetBoard);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();
    var publicId = Route<string>("publicId");

    if (string.IsNullOrWhiteSpace(publicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
      return;
    }

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status403Forbidden, cancellation: ct);
      return;
    }

    var command = new SetFixedBoardLayoutCommand
    {
      GuestDraftPublicId = publicId,
      CallerUserPublicId = userPublicId,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
