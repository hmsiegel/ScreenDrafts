namespace ScreenDrafts.Modules.Movies.Features.Movies.GetMediaPerson;

internal sealed class Endpoint : ScreenDraftsEndpoint<GetMediaPersonRequest, MediaPersonProfileResponse>
{
  public override void Configure()
  {
    Get(MediaPersonRoutes.GetPerson);
    Description(x =>
    {
      x.WithTags(MoviesOpenApi.Tags.Media)
        .WithName(MediaPersonRoutes.GetPersonOperationName)
        .Produces<MediaPersonProfileResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
    AllowAnonymous();
  }

  public override async Task HandleAsync(GetMediaPersonRequest req, CancellationToken ct)
  {
    var result = await Sender.Send(new GetMediaPersonQuery { Id = req.Id }, ct);
    await this.SendOkAsync(result, ct);
  }
}
