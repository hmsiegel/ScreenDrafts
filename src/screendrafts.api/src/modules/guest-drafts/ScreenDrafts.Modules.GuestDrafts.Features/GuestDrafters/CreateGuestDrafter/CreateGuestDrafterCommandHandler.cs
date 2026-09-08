using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.CreateGuestDrafter;

internal sealed class CreateGuestDrafterCommandHandler(
  IDrafterRepository guestDrafterRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<CreateGuestDrafterCommand, string>
{
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result<string>> Handle(
    CreateGuestDrafterCommand request,
    CancellationToken cancellationToken
  )
  {
    var existing = await _guestDrafterRepository.GetByUserIdAsync(
      request.UserId,
      cancellationToken
    );

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

    _guestDrafterRepository.Add(result.Value);

    return Result.Success(result.Value.PublicId);
  }
}
