namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class GetDraft(ISender sender) : Endpoint<GetDraftRequest, DraftResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{id}");
    Description(x => {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(GetDraft))
      .WithDescription("Gets a draft by it's Id.");
      });
    Policies(Presentation.Permissions.GetDrafts);
  }

  public override async Task HandleAsync(GetDraftRequest req, CancellationToken ct)
  {
    var query = new GetDraftQuery(req.Id);
    var draft = await _sender.Send(query, ct);
    await SendOkAsync(draft.Value!, ct);
  }
}
public sealed record GetDraftRequest(
    [FromRoute(Name = "id")] Guid Id);


internal sealed class GetDraftSummary : Summary<GetDraft>
{ public GetDraftSummary()
  {
    Summary = "Get a draft by Id";
    Description = "Gets a draft by Id. The draft must exist and the user must have permission to view it.";
    Response<DraftResponse>(StatusCodes.Status200OK, "The draft.");
    Response(StatusCodes.Status404NotFound, "Draft not found.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to view this draft.");
  }
}
