namespace ScreenDrafts.Modules.Drafts.Presentation.Drafters;

internal sealed class GetDrafter(ISender sender) : EndpointWithoutRequest<DrafterResponse>
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Get("/drafters/{id}");
    Description(x => x.WithTags(Presentation.Tags.Drafters));
    Policies(Presentation.Permissions.GetDrafters);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var id = Route<Guid>("id");
    var query = new GetDrafterQuery(id);
    var drafter = await _sender.Send(query, ct);
    await SendOkAsync(drafter.Value!, ct);
  }
}

