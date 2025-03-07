namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ExecuteVetoOverride(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{drafterId:guid}/vetooveride/{vetoId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.VetoOverride);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var drafterId = Route<Guid>("drafterId");
    var vetoId = Route<Guid>("vetoId");


    var command = new ExecuteVetoOverrideCommand(drafterId, vetoId);

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
