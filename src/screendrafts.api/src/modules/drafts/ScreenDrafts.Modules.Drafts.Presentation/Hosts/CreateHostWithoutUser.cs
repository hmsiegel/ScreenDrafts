namespace ScreenDrafts.Modules.Drafts.Presentation.Hosts;
internal sealed class CreateHostWithoutUser(ISender sender) : Endpoint<HostRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/hosts");
    Description(x => x.WithTags(Presentation.Tags.Hosts));
    AllowAnonymous();
  }

  public override async Task HandleAsync(HostRequest req, CancellationToken ct)
  {
    var command = new CreateHostWithoutUserCommand(req.Name);

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

public sealed record HostRequest(string Name);
