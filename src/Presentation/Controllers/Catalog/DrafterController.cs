namespace ScreenDrafts.Presentation.Controllers.Catalog;
public class DrafterController : VersionedApiController
{
    [HttpPost]
    [MustHavePermission(ScreenDraftsAction.Create, ScreenDraftsResource.Drafters)]
    [OpenApiOperation("Create a new drafter.", "")]
    public async Task<IActionResult> CreateDrafter(CreateDrafterRequest request, CancellationToken cancellationToken = default)
    {
        var command = Mapper.Map<CreateDrafterCommand>(request);
        var result = await Mediator.Send(command, cancellationToken);

        return Ok(result);
    }
}
