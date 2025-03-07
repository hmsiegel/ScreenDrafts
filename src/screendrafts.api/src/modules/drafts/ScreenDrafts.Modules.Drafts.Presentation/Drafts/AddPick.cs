namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class AddPick(ISender sender) : Endpoint<AddPickRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/picks/{drafterId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Picks));
    Policies(Presentation.Permissions.ModifyDraft);
  }
  public override async Task HandleAsync(AddPickRequest req, CancellationToken ct)
  {
    var draftId = Route<Guid>("draftId");
    var drafterId = Route<Guid>("drafterId");

    ArgumentNullException.ThrowIfNull(req);
    var command = new AddPickCommand(
      DraftId: draftId,
      Position: req.Position,
      MovieId: req.MovieId,
      DrafterId: drafterId);

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
  int Position,
  Guid MovieId);
