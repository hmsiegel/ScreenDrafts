namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.CreateDrafter;

internal sealed class CreateDrafterCommandHandler(
  IDrafterRepository drafterRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateDrafterCommand, string>
{
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateDrafterCommand request,
    CancellationToken cancellationToken
  )
  {
    var existing = await _drafterRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    if (existing is not null)
    {
      return Result.Failure<string>(DrafterErrors.AlreadyExistsForUser(request.UserId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDrafter);

    var result = Drafter.Create(publicId, request.UserId, request.FirstName, request.LastName);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    _drafterRepository.Add(result.Value);

    return Result.Success(result.Value.PublicId);
  }
}
