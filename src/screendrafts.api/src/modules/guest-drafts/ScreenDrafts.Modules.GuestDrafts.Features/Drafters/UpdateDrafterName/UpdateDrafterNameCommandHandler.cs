namespace ScreenDrafts.Modules.GuestDrafts.Features.Drafters.UpdateDrafterName;

internal sealed class UpdateDrafterNameCommandHandler(
  IDrafterRepository drafterRepository,
  IPublicIdGenerator publicIdGenerator
) : ICommandHandler<UpdateDrafterNameCommand>
{
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public async Task<Result> Handle(
    UpdateDrafterNameCommand request,
    CancellationToken cancellationToken
  )
  {
    var drafter = await _drafterRepository.GetByUserIdAsync(request.UserId, cancellationToken);

    if (drafter is null)
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

      _drafterRepository.Add(createResult.Value);

      return Result.Success();
    }

    var result = drafter.UpdateName(request.FirstName, request.LastName);

    if (result.IsFailure)
    {
      return result;
    }

    _drafterRepository.Update(drafter);

    return Result.Success();
  }
}
