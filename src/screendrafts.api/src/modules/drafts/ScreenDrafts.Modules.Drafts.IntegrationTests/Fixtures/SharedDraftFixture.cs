using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures.Create;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.Fixtures;

/// <summary>
/// Seeds the reference data shared across all scenario tests:
/// - Series ("Regular")
/// - Categories ("Director", "Year Draft", "Decade Drafts", "Director Drafts")
/// - Hosts (Clay Keller, Ryan Marker, Darren Franich, Phil Iscove, Bryan Cogman)
///
/// Call <see cref="SeedAsync"/> once from each scenario test class's InitializeAsync.
/// Uses an idempotency flag so re-seeding within the same factory lifetime is a no-op.
/// </summary>
public sealed class SharedDraftFixture
{
  // Series
  public Guid RegularSeriesId { get; private set; }

  // Categories
  public string DirectorCategoryPublicId { get; private set; } = default!;
  public string YearDraftCategoryPublicId { get; private set; } = default!;
  public string DecadeDraftsCategoryPublicId { get; private set; } = default!;
  public string DirectorDraftsCategoryPublicId { get; private set; } = default!;

  // Person publicIds (used to look up both Host and Drafter roles from same Person)
  public string ClayPersonPublicId { get; private set; } = default!;
  public string RyanPersonPublicId { get; private set; } = default!;
  public string DarrenPersonPublicId { get; private set; } = default!;
  public string PhilPersonPublicId { get; private set; } = default!;
  public string BryanPersonPublicId { get; private set; } = default!;

  // Host publicIds
  public string ClayHostPublicId { get; private set; } = default!;
  public string RyanHostPublicId { get; private set; } = default!;
  public string DarrenHostPublicId { get; private set; } = default!;
  public string PhilHostPublicId { get; private set; } = default!;
  public string BryanHostPublicId { get; private set; } = default!;

  private bool _seeded;

  public async Task SeedAsync(ISender sender)
  {
    ArgumentNullException.ThrowIfNull(sender);
    if (_seeded) return;

    RegularSeriesId = await CreateSeriesAsync(sender, "Regular");

    DirectorCategoryPublicId = await CreateCategoryAsync(sender, "Director");
    YearDraftCategoryPublicId = await CreateCategoryAsync(sender, "Year Draft");
    DecadeDraftsCategoryPublicId = await CreateCategoryAsync(sender, "Decade Drafts");
    DirectorDraftsCategoryPublicId = await CreateCategoryAsync(sender, "Director Drafts");

    (ClayPersonPublicId, ClayHostPublicId) = await CreatePersonAndHostAsync(sender, "Clay", "Keller");
    (RyanPersonPublicId, RyanHostPublicId) = await CreatePersonAndHostAsync(sender, "Ryan", "Marker");
    (DarrenPersonPublicId, DarrenHostPublicId) = await CreatePersonAndHostAsync(sender, "Darren", "Franich");
    (PhilPersonPublicId, PhilHostPublicId) = await CreatePersonAndHostAsync(sender, "Phil", "Iscove");
    (BryanPersonPublicId, BryanHostPublicId) = await CreatePersonAndHostAsync(sender, "Bryan", "Cogman");

    _seeded = true;
  }

  private static async Task<Guid> CreateSeriesAsync(ISender sender, string name)
  {
    var result = await sender.Send(new CreateSeriesCommand
    {
      Name = name,
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    });
    return result.Value;
  }

  private static async Task<string> CreateCategoryAsync(ISender sender, string name)
  {
    var result = await sender.Send(new CreateCategoryCommand
    {
      Name = name,
      Description = name
    });
    return result.Value;
  }

  private static async Task<(string personPublicId, string hostPublicId)> CreatePersonAndHostAsync(
    ISender sender,
    string firstName,
    string lastName)
  {
    var personResult = await sender.Send(new CreatePersonCommand
    {
      FirstName = firstName,
      LastName = lastName,
      PublicId = $"p_{Guid.NewGuid():N}"
    });
    var personPublicId = personResult.Value;

    var hostResult = await sender.Send(new CreateHostCommand { PersonPublicId = personPublicId });
    return (personPublicId, hostResult.Value);
  }
}
