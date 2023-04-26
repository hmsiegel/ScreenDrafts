namespace ScreenDrafts.Presentation.Controllers.Catalog;

// TODO: Make sure to change this to a VersionedApiController when you start versioning your API.
public sealed class DraftsController : VersionedApiController
{
    [HttpPost]
    [MustHavePermission(ScreenDraftsAction.Create, ScreenDraftsResource.Drafts)]
    [OpenApiOperation("Create a new draft.", "")]
    public Task CreateDraft(CreateDraftRequest request)
    {
        var command = Mapper.Map<CreateDraftCommand>(request);
        var response = Mediator.Send(command);
        return response;
    }
}
