namespace ScreenDrafts.Modules.Drafts.Features.DraftParts.SubDrafts.GetSubDraftGameplay;

internal sealed class Endpoint : ScreenDraftsEndpointWithoutRequest<GetSubDraftGameplayResponse>
{
  public override void Configure()
  {
    Get(DraftPartRoutes.SubDraftGameplay);
    Description(x =>
    {
      x.WithTags(DraftsOpenApi.Tags.DraftParts)
        .WithName(DraftsOpenApi.Names.SubDrafts_GetGameplay)
        .Produces<GetSubDraftGameplayResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    });
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var draftPartPublicId = Route<string>("draftPartId");
    var subDraftPublicId = Route<string>("subDraftId");

    if (string.IsNullOrWhiteSpace(draftPartPublicId) || string.IsNullOrWhiteSpace(subDraftPublicId))
    {
      await Send.ErrorsAsync(StatusCodes.Status400BadRequest, ct);
      return;
    }

    var query = new GetSubDraftGameplayQuery
    {
      DraftPartPublicId = draftPartPublicId,
      SubDraftPublicId = subDraftPublicId,
    };

    var result = await Sender.Send(query, ct);
    await this.SendOkAsync(result, ct);
  }
}
