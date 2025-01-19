namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

public class ListDraftsEndpoint(ISender sender) : EndpointWithoutRequest<Results<Ok<List<DraftResponse>>, NoContent>>

{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    AllowAnonymous();
  }


  public override async Task<Results<Ok<List<DraftResponse>>, NoContent>> ExecuteAsync(CancellationToken ct)
  {

    var query = new ListDraftsQuery();

    var drafts = await _sender.Send(query, ct);

    if (drafts.Any())
    {
      return TypedResults.Ok(drafts.ToList());
    }
    else
    {
      return TypedResults.NoContent();
    }
  }
}
