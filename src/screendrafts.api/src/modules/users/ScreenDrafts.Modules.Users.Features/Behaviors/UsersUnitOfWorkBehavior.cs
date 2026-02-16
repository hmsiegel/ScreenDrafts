using ScreenDrafts.Modules.Users.Domain.Abstractions.Data;

namespace ScreenDrafts.Modules.Users.Features.Behaviors;

public sealed class UsersUnitOfWorkBehavior<TRequest, TResponse>
  (IUnitOfWork unitOfWork)
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
