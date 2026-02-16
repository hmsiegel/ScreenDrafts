using ScreenDrafts.Common.Application.Services;
using ScreenDrafts.Modules.Drafts.Domain.People;

namespace ScreenDrafts.Seeding.Users.Seeders;

internal sealed class PeopleSeeder(
  ILogger<PeopleSeeder> logger,
  ICsvFileService csvFileService,
  UsersDbContext dbContext,
  DraftsDbContext draftsDbContext,
  IPublicIdGenerator publicIdGenerator)
  : UserBaseSeeder(
    logger,
    csvFileService,
    dbContext), ICustomSeeder
{
  private readonly DraftsDbContext _draftsDbContext = draftsDbContext;
  private readonly IPublicIdGenerator _publicIdGenerator = publicIdGenerator;

  public int Order => 2;
  public string Name => "people";

  public Task InitializeAsync(CancellationToken cancellationToken = default)
    => SeedPeopleFromUsersAsync(cancellationToken);

  private async Task SeedPeopleFromUsersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "People";

    var users = await _dbContext.Users
      .AsNoTracking()
      .Where(u => u.PersonId != null)
      .Select(u => new { UserId = u.Id, u.PersonId, FirstName = u.FirstName.Value, LastName = u.LastName.Value })
      .ToListAsync(cancellationToken);

    if (users.Count == 0)
    {
      return;
    }

    var personIds = users.Select(u => PersonId.Create(u.PersonId!.Value)).ToHashSet();

    var existingPeople = await _draftsDbContext.People
      .Where(p => personIds.Contains(p.Id))
      .ToDictionaryAsync(p => p.Id, cancellationToken);

    foreach (var u in users)
    {
      var displayName = string.IsNullOrWhiteSpace(u.LastName)
        ? u.FirstName!
        : $"{u.FirstName} {u.LastName}".Trim();
      var publicId = _publicIdGenerator.GeneratePublicId(PublicIdPrefixes.Person);
      var personId = PersonId.Create(u.PersonId!.Value);

      if (existingPeople.TryGetValue(personId, out var person))
      {
        person.Update(u.FirstName!, u.LastName!, displayName);
      }
      else
      {
        var personResult = Person.Create(
          publicId: publicId,
          firstName: u.FirstName!,
          lastName: u.LastName!,
          userId: u.UserId.Value,
          id: u.PersonId.Value,
          displayName: displayName);

        if (personResult.IsFailure)
        {
          DatabaseSeedingLoggingMessages.UnableToResolve(_logger, $"User with ID {u.UserId} to Person: {personResult.Error}");
          continue;
        }

        var newPerson = personResult.Value;

        _draftsDbContext.People.Add(newPerson);

      }
    }
    await _draftsDbContext.SaveChangesAsync(cancellationToken);
    LogInsertComplete(TableName, users.Count);
  }
}
