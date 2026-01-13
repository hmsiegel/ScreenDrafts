using ScreenDrafts.Common.Features.Abstractions.Messaging;

namespace ScreenDrafts.Modules.Drafts.Features.Behaviors;

public sealed class DraftsUnitOfWorkBehavior<TRequest, TResponse>
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
