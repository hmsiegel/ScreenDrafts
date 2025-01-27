namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class ListDrafts(ISender sender) : EndpointWithoutRequest<Result<List<DraftResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListDraftsQuery();

    var drafts = (await _sender.Send(query, ct)).Value.ToList();

    if (drafts.Count != 0)
    {
      await SendOkAsync(Result.Success(drafts), ct);
    }
    else
    {
      await SendNoContentAsync(ct);
    }
  }
}
