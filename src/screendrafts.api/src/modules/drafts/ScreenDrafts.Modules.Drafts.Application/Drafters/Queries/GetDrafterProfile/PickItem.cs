
namespace ScreenDrafts.Modules.Drafts.Application.Drafters.Queries.GetDrafterProfile;

public sealed record PickItem(
  Guid PickId,
  int Position,
  int PlayOrder,
  Guid MovieId,
  string MovieTitle,
  bool WasVetoed,
  bool WasVetoOverride,
  bool WasCommissionerOverride,
  Guid? VetoedById,
  string? VetoedByName,
  Guid? VetoOverrideById,
  string? VetoOverrideByName);
