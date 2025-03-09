namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class GetDraftPicks(ISender sender) : EndpointWithoutRequest<List<DraftPickResponse>>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{draftId:guid}/picks");
    Description(x => x.WithTags(Presentation.Tags.Picks));
    Policies(Presentation.Permissions.GetDrafts);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var query = new GetDraftPicksByDraftQuery(draftId);

    var result = await _sender.Send(query, ct);

    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(result.Value, ct);
    }
  }
}
