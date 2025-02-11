namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class CreateDrafter(ISender sender) : Endpoint<CreateDrafterRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters/create");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.CreateDrafter);
  }
  public override async Task HandleAsync(CreateDrafterRequest req, CancellationToken ct)
  {
    var command = new CreateDrafterCommand(req.UserId, req.Name);

    var drafterId = await _sender.Send(command, ct);

    if (drafterId.IsFailure)
    {
      await SendErrorsAsync(StatusCodes.Status400BadRequest, ct);
    }
    else
    {
      await SendOkAsync(drafterId.Value, ct);
    }
  }
}

public sealed record CreateDrafterRequest(Guid? UserId, string? Name);
