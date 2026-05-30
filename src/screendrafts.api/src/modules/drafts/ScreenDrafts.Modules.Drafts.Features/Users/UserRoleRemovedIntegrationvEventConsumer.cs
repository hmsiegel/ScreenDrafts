namespace ScreenDrafts.Modules.Drafts.Features.Users;

internal sealed class UserRoleRemovedIntegrationvEventConsumer(
  IPersonRepository personRepository,
  IDrafterRepository drafterRepository,
  IHostRepository hostRepository
) : IntegrationEventHandler<UserRoleRemovedIntegrationEvent>
{
  private readonly IPersonRepository _personRepository = personRepository;
  private readonly IDrafterRepository _drafterRepository = drafterRepository;
  private readonly IHostRepository _hostRepository = hostRepository;

  private const string DrafterRole = "Drafter";
  private const string HostRole = "Host";

  public override async Task Handle(
    UserRoleRemovedIntegrationEvent integrationEvent,
    CancellationToken cancellationToken = default
  )
  {
    if (integrationEvent.RoleName is not (DrafterRole or HostRole))
    {
      return;
    }

    var person = await _personRepository.GetByUserIdAsync(
      integrationEvent.UserId,
      cancellationToken
    );

    if (person is null)
    {
      return;
    }

    if (integrationEvent.RoleName == DrafterRole)
    {
      var drafter = await _drafterRepository.GetByPersonPublicIdAsync(
        person.PublicId,
        cancellationToken
      );

      if (drafter is null || drafter.IsRetired)
      {
        return;
      }

      var result = drafter.RetireDrafter();

      if (result.IsFailure)
      {
        throw new ScreenDraftsException(
          $"Failed to retire drafter for person with public id {person.PublicId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
        );
      }

      _drafterRepository.Update(drafter);
    }
    else if (integrationEvent.RoleName == HostRole)
    {
      var host = await _hostRepository.GetByPersonPublicIdAsync(person.PublicId, cancellationToken);
      if (host is null || host.IsRetired)
      {
        return;
      }
      var result = host.Retire();
      if (result.IsFailure)
      {
        throw new ScreenDraftsException(
          $"Failed to retire host for person with public id {person.PublicId}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}"
        );
      }
      _hostRepository.Update(host);
    }
  }
}
