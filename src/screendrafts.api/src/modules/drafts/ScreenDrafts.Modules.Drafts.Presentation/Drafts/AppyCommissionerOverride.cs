namespace ScreenDrafts.Modules.Drafts.Presentation.Drafts;

internal sealed class AppyCommissionerOverride(ISender sender) : EndpointWithoutRequest
{
  private readonly ISender _sender = sender;

  public override void Configure()
  {
    Post("/drafts/{draftId:guid}/commissioner-override/{pickId:guid}");
    Description(x => x.WithTags(Presentation.Tags.Drafts));
    Policies(Presentation.Permissions.ModifyDraft);
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var pickId = Route<Guid>("pickId");

    var command = new ApplyCommissionerOverrideCommand(pickId);

    var result = await _sender.Send(command, ct);

    await this.MapResultsAsync(result, ct);
  }
}

