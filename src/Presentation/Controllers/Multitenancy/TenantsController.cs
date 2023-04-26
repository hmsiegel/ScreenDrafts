namespace ScreenDrafts.Presentation.Controllers.Multitenancy;

public class TenantsController : VersionNeutralApiController
{
    [HttpGet]
    [MustHavePermission(ScreenDraftsAction.View, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Get a list of all tenants.", "")]
    public Task<List<TenantDto>> GetListAsync()
    {
        return Mediator.Send(new GetAllTenantsRequest());
    }

    [HttpGet("{id}")]
    [MustHavePermission(ScreenDraftsAction.View, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Get tenant details.", "")]
    public Task<TenantDto> GetAsync(string id)
    {
        return Mediator.Send(new GetTenantRequest(id));
    }

    [HttpPost]
    [MustHavePermission(ScreenDraftsAction.Create, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Create a new tenant.", "")]
    public Task<string> CreateAsync(CreateTenantRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPost("{id}/activate")]
    [MustHavePermission(ScreenDraftsAction.Update, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Activate a tenant.", "")]
    [ApiConventionMethod(typeof(ScreenDraftsApiConvention), nameof(ScreenDraftsApiConvention.Register))]
    public Task<string> ActivateAsync(string id)
    {
        return Mediator.Send(new ActivateTenantRequest(id));
    }

    [HttpPost("{id}/deactivate")]
    [MustHavePermission(ScreenDraftsAction.Update, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Deactivate a tenant.", "")]
    [ApiConventionMethod(typeof(ScreenDraftsApiConvention), nameof(ScreenDraftsApiConvention.Register))]
    public Task<string> DeactivateAsync(string id)
    {
        return Mediator.Send(new DeactivateTenantRequest(id));
    }

    [HttpPost("{id}/upgrade")]
    [MustHavePermission(ScreenDraftsAction.UpgradeSubscription, ScreenDraftsResource.Tenants)]
    [OpenApiOperation("Upgrade a tenant's subscription.", "")]
    [ApiConventionMethod(typeof(ScreenDraftsApiConvention), nameof(ScreenDraftsApiConvention.Register))]
    public async Task<ActionResult<string>> UpgradeSubscriptionAsync(string id, UpgradeSubscriptionRequest request)
    {
        return id != request.TenantId
            ? BadRequest()
            : Ok(await Mediator.Send(request));
    }
}