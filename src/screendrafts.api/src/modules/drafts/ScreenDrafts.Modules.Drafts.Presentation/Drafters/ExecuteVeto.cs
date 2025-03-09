namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ExecuteVeto(ISender sender) : EndpointWithoutRequest
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/veto/{pickId:guid}/{drafterId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.VetoPicks);
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var drafterId = Route<Guid>("drafterId");
    var pickId = Route<Guid>("pickId");
    var command = new ExecuteVetoCommand(drafterId, pickId, draftId);
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
