namespace ScreenDrafts.Presentation.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    private ISender _mediator = null!;
    private IMapper _mapper = null!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();
}