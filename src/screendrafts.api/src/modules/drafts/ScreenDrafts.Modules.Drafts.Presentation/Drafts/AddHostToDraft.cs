namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;
internal sealed class AddHostToDraft(ISender sender) : EndpointWithoutRequest
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/hosts/{hostId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var hostId = Route<Guid>("hostId");

    var command = new AddHostToDraftCommand(draftId, hostId);

    var result = await _sender.Send(command, ct);

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

