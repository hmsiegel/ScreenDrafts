namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class ExecuteVeto(ISender sender) : Endpoint<ExecuteVetoRequest>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/veto/{pickId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.VetoPicks);
  }
  public override async Task HandleAsync(ExecuteVetoRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var pickId = Route<Guid>("pickId");
    var command = new ExecuteVetoCommand(req.DrafterTeamId, req.DrafterId, pickId, draftId);
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

public sealed record ExecuteVetoRequest(Guid? DrafterId, Guid? DrafterTeamId);
