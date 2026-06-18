using ScreenDrafts.Modules.Drafts.Features.Categories.Create;
using ScreenDrafts.Modules.Drafts.Features.People.LinkUser;
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
  public string RegularSeriesId { get; private set; } = default!;

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

  // User publicIds — required by RevealPickCommand.UserPublicId, which is
  // resolved User -> Person -> Host. NOT the same as the HostPublicId
  // properties above; do not substitute one for the other.
  public string ClayHostUserPublicId { get; private set; } = default!;
  public string RyanHostUserPublicId { get; private set; } = default!;
  public string DarrenHostUserPublicId { get; private set; } = default!;
  public string PhilHostUserPublicId { get; private set; } = default!;
  public string BryanHostUserPublicId { get; private set; } = default!;

  private bool _seeded;

  public async Task SeedAsync(ISender sender, FakeUsersApi fakeUsersApi)
  {
    ArgumentNullException.ThrowIfNull(sender);
    ArgumentNullException.ThrowIfNull(fakeUsersApi);

    if (_seeded)
      return;

    RegularSeriesId = await CreateSeriesAsync(sender, "Regular");

    DirectorCategoryPublicId = await CreateCategoryAsync(sender, "Director");
    YearDraftCategoryPublicId = await CreateCategoryAsync(sender, "Year Draft");
    DecadeDraftsCategoryPublicId = await CreateCategoryAsync(sender, "Decade Drafts");
    DirectorDraftsCategoryPublicId = await CreateCategoryAsync(sender, "Director Drafts");

    (ClayPersonPublicId, ClayHostPublicId) = await CreatePersonAndHostAsync(
      sender,
      "Clay",
      "Keller"
    );
    (RyanPersonPublicId, RyanHostPublicId) = await CreatePersonAndHostAsync(
      sender,
      "Ryan",
      "Marker"
    );
    (DarrenPersonPublicId, DarrenHostPublicId) = await CreatePersonAndHostAsync(
      sender,
      "Darren",
      "Franich"
    );
    (PhilPersonPublicId, PhilHostPublicId) = await CreatePersonAndHostAsync(
      sender,
      "Phil",
      "Iscove"
    );
    (BryanPersonPublicId, BryanHostPublicId) = await CreatePersonAndHostAsync(
      sender,
      "Bryan",
      "Cogman"
    );

    ClayHostUserPublicId = await LinkPersonToNewUserAsync(sender, fakeUsersApi, ClayPersonPublicId);
    RyanHostUserPublicId = await LinkPersonToNewUserAsync(sender, fakeUsersApi, RyanPersonPublicId);
    DarrenHostUserPublicId = await LinkPersonToNewUserAsync(
      sender,
      fakeUsersApi,
      DarrenPersonPublicId
    );
    PhilHostUserPublicId = await LinkPersonToNewUserAsync(sender, fakeUsersApi, PhilPersonPublicId);
    BryanHostUserPublicId = await LinkPersonToNewUserAsync(
      sender,
      fakeUsersApi,
      BryanPersonPublicId
    );

    _seeded = true;
  }

  private static async Task<string> CreateSeriesAsync(ISender sender, string name)
  {
    var result = await sender.Send(
      new CreateSeriesCommand
      {
        Name = name,
        Kind = SeriesKind.Regular.Value,
        CanonicalPolicy = CanonicalPolicy.Always.Value,
        ContinuityScope = ContinuityScope.None.Value,
        ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
        AllowedDraftTypes = (int)DraftTypeMask.All,
        DefaultDraftType = DraftType.Standard.Value,
      },
      TestContext.Current.CancellationToken
    );
    return result.Value;
  }

  private static async Task<string> CreateCategoryAsync(ISender sender, string name)
  {
    var result = await sender.Send(
      new CreateCategoryCommand { Name = name, Description = name },
      TestContext.Current.CancellationToken
    );
    return result.Value;
  }

  private static async Task<(string personPublicId, string hostPublicId)> CreatePersonAndHostAsync(
    ISender sender,
    string firstName,
    string lastName
  )
  {
    var personResult = await sender.Send(
      new CreatePersonCommand
      {
        FirstName = firstName,
        LastName = lastName,
        PublicId = $"p_{Guid.NewGuid():N}",
      }
    );
    var personPublicId = personResult.Value;

    var hostResult = await sender.Send(
      new CreateHostCommand { PersonPublicId = personPublicId },
      TestContext.Current.CancellationToken
    );
    return (personPublicId, hostResult.Value);
  }

  /// <summary>
  /// Registers a fake User and links it to the given Person via
  /// LinkUserPersonCommand. Returns the new User's public id — the value
  /// RevealPickCommand.UserPublicId actually expects, NOT the Person or
  /// Host public id.
  /// </summary>
  private static async Task<string> LinkPersonToNewUserAsync(
    ISender sender,
    FakeUsersApi fakeUsersApi,
    string personPublicId
  )
  {
    var userId = Guid.NewGuid();
    var userPublicId = fakeUsersApi.RegisterUser(userId, $"u_{Guid.NewGuid():N}"[..18]);

    var linkResult = await sender.Send(
      new LinkUserPersonCommand { PublicId = personPublicId, UserId = userId },
      TestContext.Current.CancellationToken
    );

    if (linkResult.IsFailure)
    {
      throw new InvalidOperationException(
        $"SharedDraftFixture setup failed to link person '{personPublicId}' to a fake user: "
          + string.Join("; ", linkResult.Errors.Select(e => e.Description))
      );
    }

    return userPublicId;
  }
}
