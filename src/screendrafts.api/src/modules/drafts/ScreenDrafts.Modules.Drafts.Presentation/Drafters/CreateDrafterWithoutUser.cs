namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class CreateDrafterWithoutUser(ISender sender) : Endpoint<DrafterRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafters");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    AllowAnonymous();
  }

  public override async Task HandleAsync(DrafterRequest req, CancellationToken ct)
  {
    var command = new CreateDrafterWithoutUserCommand(req.Name);

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

public sealed record DrafterRequest(
  string Name);
