namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class GetDraft(ISender sender) : EndpointWithoutRequest<DraftResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafts/{id}");
    AllowAnonymous();
    Description(x => x.WithTags(Presentation.Tags.Drafts));
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var id = Route<Guid>("id");

    var query = new GetDraftQuery(id);

    var draft = await _sender.Send(query, ct);

    await SendOkAsync(draft.Value!, ct);
  }
}

public sealed record GetDraftRequest(Guid Id);
