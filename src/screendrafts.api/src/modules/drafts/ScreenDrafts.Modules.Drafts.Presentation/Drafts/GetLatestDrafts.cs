namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class GetLatestDrafts(ISender sender) : EndpointWithoutRequest<List<DraftResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/latest");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var query = new GetLatestDraftsQuery();
    var result = await _sender.Send(query, ct);
    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      var draftList = result.Value.ToList();
      await SendOkAsync(draftList, ct);
    }
  }
}
