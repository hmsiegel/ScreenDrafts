namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class RemoveHostFromDraft(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/remove-host/{hostId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var hostId = Route<Guid>("hostId");

    var command = new RemoveHostFromDraftCommand(draftId, hostId);
    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}
