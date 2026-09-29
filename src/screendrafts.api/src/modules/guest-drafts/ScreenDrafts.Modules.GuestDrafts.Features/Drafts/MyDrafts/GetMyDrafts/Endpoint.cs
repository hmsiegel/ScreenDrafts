namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafts.MyDrafts.GetMyDrafts;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetMyGuestDraftsResponse>
{
  public override void Configure()
  {
    Get(MyGuestDraftsRoutes.Base);
    Description(x =>
      x.WithTags(GuestDraftsOpenApi.Tags.MyDrafts)
        .WithName(GuestDraftsOpenApi.Names.GuestDrafts_GetMyDrafts)
        .Produces<GetMyGuestDraftsResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
    );
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var userPublicId = User.GetUserPublicId();

    if (userPublicId is null)
    {
      await Send.ErrorsAsync(StatusCodes.Status401Unauthorized, cancellation: ct);
      return;
    }

    var query = new GetMyDraftsQuery { CallerUserPublicId = userPublicId };

    var result = await Sender.Send(query, ct);

    await this.SendOkAsync(result, ct);
  }
}
