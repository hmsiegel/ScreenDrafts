namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.CreateGuestDrafter;

internal sealed class CreateGuestDrafterCommandHandler(
  IGuestDrafterRepository guestDrafterRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateGuestDrafterCommand, string>
{
  private readonly IGuestDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateGuestDrafterCommand request,
    CancellationToken cancellationToken)
  {
    var existing = await _guestDrafterRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    if (existing is not null)
    {
      return Result.Failure<string>(GuestDrafterErrors.AlreadyExistsForUser(request.UserId));
    }

    var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDrafter);

    var result = GuestDrafter.Create(publicId, request.UserId, request.FirstName, request.LastName);

    if (result.IsFailure)
    {
      return Result.Failure<string>(result.Errors);
    }

    _guestDrafterRepository.Add(result.Value);

    return Result.Success(result.Value.PublicId);
  }
}
