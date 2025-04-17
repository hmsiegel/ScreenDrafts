namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ExecuteVetoOverride(ISender sender) : Endpoint<VetoOverrideRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{vetoId:guid}/vetooveride");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.VetoOverride);
  }

  public override async Task HandleAsync(VetoOverrideRequest req, CancellationToken ct)
  {
    var vetoId = Route<Guid>("vetoId");

    var command = new ExecuteVetoOverrideCommand(req.DrafterId, req.DrafterTeamId, vetoId);

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

public sealed record VetoOverrideRequest(Guid? DrafterId, Guid? DrafterTeamId, Guid VetoId); 
