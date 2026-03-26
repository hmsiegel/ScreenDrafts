using ScreenDrafts.Modules.Movies.Domain.Abstractions.Data;

namespace ScreenDrafts.Modules.Movies.Features.Behaviors;

public sealed class MoviesUnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
  : IPipelineBehavior<TRequest, TResponse>
  where TRequest : IBaseCommand
{
  private readonly IUnitOfWork _unitOfWork = unitOfWork;

  public async Task<TResponse> Handle(
    TRequest request,
    RequestHandlerDelegate<TResponse> next,
    CancellationToken cancellationToken)
  {
    ArgumentNullException.ThrowIfNull(next);

    var response = await next(cancellationToken);

    await _unitOfWork.SaveChangesAsync(cancellationToken);

    return response;
  }
}
