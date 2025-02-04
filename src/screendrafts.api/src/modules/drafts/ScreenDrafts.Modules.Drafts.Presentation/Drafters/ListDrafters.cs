namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ListDrafters(ISender sender) : EndpointWithoutRequest<Result<List<DrafterResponse>>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafters");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.GetDrafters);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new ListDraftersQuery();

    var drafters = (await _sender.Send(query, ct)).Value.ToList();

    if (drafters.Count != 0)
    {
      await SendOkAsync(Result.Success(drafters), ct);
    }
    else
    {
      await SendNoContentAsync(ct);
    }
  }
}
