namespace ScreenDrafts.Presentation.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    private ISender _mediator = null!;
    private IMapper _mapper = null!;

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    protected IMapper Mapper => _mapper ??= HttpContext.RequestServices.GetRequiredService<IMapper>();

    protected IActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        HttpContext.Items[HttpContextItemKey.Errors] = errors;

        return Problem(errors[0]);
    }

    private IActionResult Problem(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Problem(statusCode: statusCode, title: error.Description);
    }

    private IActionResult ValidationProblem(List<Error> errors)
    {
        var modelStateDictrionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictrionary.AddModelError(
                error.Code,
                error.Description);
        }

        return ValidationProblem(modelStateDictrionary);
    }
}