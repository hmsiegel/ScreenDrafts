using FromQueryAttribute = Microsoft.AspNetCore.Mvc.FromQueryAttribute;

namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts.Posts;

internal sealed class AddHostToDraft(ISender sender) : Endpoint<AddHostToDraftRequest, Guid>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/hosts/{hostId:guid}");
    Description(x =>
    {
      x.WithTags(Presentation.Tags.Drafts)
      .WithName(nameof(AddHostToDraft))
      .WithDescription("Add a host to a draft.");
    });
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(AddHostToDraftRequest req, CancellationToken ct)
  {
    var command = new AddHostToDraftCommand(req.DraftId, req.HostId, req.Role);
    var result = await _sender.Send(command, ct);
    await this.MapResultsAsync(result, ct);
  }
}

public sealed record AddHostToDraftRequest(
    [FromRoute(Name = "draftId")] Guid DraftId,
    [FromRoute(Name = "hostId")] Guid HostId,
    [FromQuery(Name = "role")] string Role = "CoHost");

internal sealed class AddHostToDraftSummary : Summary<AddHostToDraft>
{
  public AddHostToDraftSummary()
  {
    Summary = "Add a host to a draft";
    Description = "Adds a host to a draft as a Primary host or as a Co-Host. Use the `role` query parameter.";
    Response<Guid>(StatusCodes.Status200OK, "The ID of the host added to the draft.");
    Response(StatusCodes.Status404NotFound, "Draft or host not found.");
    Response(StatusCodes.Status400BadRequest, "Invalid request (e.g. draft already has a primary host.");
    Response(StatusCodes.Status403Forbidden, "You do not have permission to add a host to this draft.");
  }
}
