using ScreenDrafts.Modules.GuestDrafts.Domain.Drafters;

namespace ScreenDrafts.Modules.GuestDrafts.Features.GuestDrafters.UpdateGuestDrafterName;

internal sealed class UpdateGuestDrafterNameCommandHandler(
  IDrafterRepository guestDrafterRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<UpdateGuestDrafterNameCommand>
{
  private readonly IDrafterRepository _guestDrafterRepository = guestDrafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    UpdateGuestDrafterNameCommand request,
    CancellationToken cancellationToken
  )
  {
    var guestDrafter = await _guestDrafterRepository.GetByUserIdAsync(
      request.UserId,
      cancellationToken
    );

    if (guestDrafter is null)
    {
      // Deliberate upsert, not a hard failure: a user who registered before
      // this consumer existed (or before GuestDrafts existed at all) will
      // never get a fresh UserRegisteredIntegrationEvent to backfill from --
      // this is the only event that will ever tell us their name again, so
      // create the row here rather than silently dropping the update.
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.GuestDrafter);

      var createResult = Drafter.Create(
        publicId,
        request.UserId,
        request.FirstName,
        request.LastName
      );

      if (createResult.IsFailure)
      {
        return Result.Failure(createResult.Errors);
      }

      _guestDrafterRepository.Add(createResult.Value);

      return Result.Success();
    }

    var result = guestDrafter.UpdateName(request.FirstName, request.LastName);

    if (result.IsFailure)
    {
      return result;
    }

    _guestDrafterRepository.Update(guestDrafter);

    return Result.Success();
  }
}
