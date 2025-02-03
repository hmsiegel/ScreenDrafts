namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class CreateDrafter(ISender sender) : EndpointWithoutRequest<Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters/{id:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    AllowAnonymous();
  }
  public override async Task HandleAsync(CancellationToken ct)
  {
    var userId = Route<Guid>("id");


    var command = new CreateDrafterCommand(userId);

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

