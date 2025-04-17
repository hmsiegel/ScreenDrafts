namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class AddPick(ISender sender) : Endpoint<AddPickRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/picks");
    Description(x => x.WithTags(Presentation.Tags.Picks));
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(AddPickRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");

    ArgumentNullException.ThrowIfNull(req);
    var command = new AddPickCommand(
      DraftId: draftId,
      Position: req.Position,
      MovieId: req.MovieId,
      DrafterId: req.DrafterId,
      DrafterTeamId: req.DrafterTeamId, 
      PlayOrder: req.PlayOrder);

    var pickId = await _sender.Send(command, ct);

    if (pickId.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(pickId.Value, ct);
    }
  }
}

public sealed record AddPickRequest(
  Guid? DrafterId,
  Guid? DrafterTeamId,
  int Position,
  int PlayOrder,
  Guid MovieId);
