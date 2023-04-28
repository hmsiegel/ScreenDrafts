namespace ScreenDrafts.Presentation.Controllers.Catalog;

public sealed class DraftsController : VersionedApiController
{
    [HttpPost]
    [MustHavePermission(ScreenDraftsAction.Create, ScreenDraftsResource.Drafts)]
    [OpenApiOperation("Create a new draft.", "")]
    public Task<DefaultIdType> CreateDraft(CreateDraftRequest request)
    {
        var command = Mapper.Map<CreateDraftCommand>(request);
        return Mediator.Send(command);
    }

    [HttpPatch("drafter")]
    [MustHavePermission(ScreenDraftsAction.Update, ScreenDraftsResource.Drafts)]
    [OpenApiOperation("Add Drafters to Draft", "")]
    public async Task<IActionResult> AddDrafterToDraft(AddDrafterRequest request, CancellationToken cancellationToken = default)
    {
        var command = Mapper.Map<AddDrafterToDraftCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return result.Match(
            success => Ok(success),
            Problem);
    }
}
