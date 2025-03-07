namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class RemoveDrafterFromDraft : EndpointWithoutRequest
{
  private readonly ISender _sender;
  public RemoveDrafterFromDraft(ISender sender)
  {
    _sender = sender;
  }
  public override void Configure()
  {
    Delete("/drafts/{draftId:guid}/drafter/{drafterId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var drafterId = Route<Guid>("drafterId");

    var command = new RemoveDrafterFromDraftCommand(
      draftId,
      drafterId);
    var result = await _sender.Send(command, ct);
    if (result.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(ct);
    }
  }
}
