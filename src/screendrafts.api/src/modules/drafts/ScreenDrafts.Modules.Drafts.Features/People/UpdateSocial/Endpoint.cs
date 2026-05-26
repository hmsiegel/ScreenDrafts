namespace ScreenDrafts.Modules.Drafts.Features.People.UpdateSocial;

internal sealed class Endpoint : ScreenDraftsEndpoint<UpdateSocialRequest>
{
  public override void Configure()
  {
    Put(PeopleRoutes.Social);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.People)
        .WithName(DraftsOpenApi.Names.People_UpdateSocial)
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status403Forbidden)
        .Produces(StatusCodes.Status404NotFound);
    });
    Policies(DraftsAuth.Permissions.PersonUpdate);
  }

  public override async Task HandleAsync(UpdateSocialRequest req, CancellationToken ct)
  {
    var command = new UpdateSocialCommand
    {
      PublicId = req.PublicId,
      TwitterHandle = req.TwitterHandle,
      InstagramHandle = req.InstagramHandle,
      LetterboxdHandle = req.LetterboxdHandle,
      BlueskyHandle = req.BlueskyHandle,
    };

    var result = await Sender.Send(command, ct);

    await this.SendNoContentAsync(result, ct);
  }
}
