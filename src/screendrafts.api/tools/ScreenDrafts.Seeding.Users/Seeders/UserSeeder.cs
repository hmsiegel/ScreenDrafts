namespace ScreenDrafts.Seeding.Users.Seeders;

internal sealed class UserSeeder(
  ILogger<UserSeeder> logger,
  ICsvFileService csvFileService,
  UsersDbContext dbContext,
  IIdentityProviderService identityProviderService,
  DraftsDbContext draftsDbContext)
  : UserBaseSeeder(logger, csvFileService, dbContext), ICustomSeeder
{
  private readonly IIdentityProviderService _identityProviderService = identityProviderService;
  private readonly DraftsDbContext _draftsDbContext = draftsDbContext;

  public int Order => 1;
  public string Name => "users";

  public async Task InitializeAsync(CancellationToken cancellationToken = default)
  {
    await SeedUsersAsync(cancellationToken);
  }

  private async Task SeedUsersAsync(CancellationToken cancellationToken)
  {
    const string TableName = "Users";

    var csvUsers = ReadCsv<UserCsvModel>(
        new SeedFile(FileNames.UsersSeeder, SeedFileType.Csv),
        TableName);

    if (csvUsers.Count == 0)
    {
      return;
    }

    var existingUserKeys = await _dbContext.Users
        .Select(u => new { u.Email })
        .ToHashSetAsync(cancellationToken);

    var existingSet = existingUserKeys
        .Select(u => (u.Email).Value)
        .ToHashSet();

    var usersToProcess = csvUsers
        .Where(record => !existingSet.Contains(record.EmailAddress))
        .ToList();

    var userResults = new List<(Guid? PersonId, User? User)>();

    // Process users in batches to avoid overwhelming Keycloak
    const int batchSize = 50;
    for (int i = 0; i < usersToProcess.Count; i += batchSize)
    {
      var batch = usersToProcess.Skip(i).Take(batchSize);
      var batchTasks = batch.Select<UserCsvModel, Task<(Guid? PersonId, User? User)>>(async record =>
      {
        var emailResult = Email.Create(record.EmailAddress);

        if (emailResult.IsFailure)
        {
          DatabaseSeedingLoggingMessages.AlreadyExists(_logger, record.EmailAddress, TableName);
          // Return nulls to indicate failure, matching the expected tuple type
          return (null, null);
        }

        var identityResult = await _identityProviderService.RegisterUserAsync(
                    new UserModel(
                        record.EmailAddress,
                        PasswordGenerator.GeneratePassword(),
                        record.FirstName,
                        record.LastName),
                    cancellationToken);

        await Task.Delay(1000, cancellationToken); // Delay to avoid overwhelming Keycloak

        if (identityResult.IsFailure)
        {
          DatabaseSeedingLoggingMessages.UnableToResolve(
                  _logger,
                  record.EmailAddress);
          return (null, null);
        }

        var userResult = User.Create(
                    email: emailResult.Value,
                    firstName: FirstName.Create(record.FirstName).Value,
                    lastName: LastName.Create(record.LastName).Value,
                    identityId: identityResult.Value,
                    personId: record.PersonId);

        if (userResult.IsFailure)
        {
          DatabaseSeedingLoggingMessages.UnableToResolve(
                  _logger,
                  record.EmailAddress);
          return (null, null);
        }

        var user = userResult.Value;

        if (!string.IsNullOrWhiteSpace(record.ProfilePictureUrl))
        {
          user.UpdateProfilePicture(record.ProfilePictureUrl);
        }

        user.UpdateSocialHandles(
                    record.TwitterHandle is not null ? record.TwitterHandle : null,
                    record.InstagramHandle is not null ? record.InstagramHandle : null,
                    record.LetterboxdHandle is not null ? record.LetterboxdHandle : null,
                    record.BlueskyHandle is not null ? record.BlueskyHandle : null);
        DatabaseSeedingLoggingMessages.ItemAddedToDatabase(_logger, user.Id.ToString());

        return (record.PersonId, user);
      });

      var batchResults = await Task.WhenAll(batchTasks);
      userResults.AddRange(batchResults);

      var currentBatch = (i / batchSize) + 1;
      var totalBatches = (usersToProcess.Count + batchSize - 1) / batchSize;
      var processedUsers = Math.Min(i + batchSize, usersToProcess.Count);
      var totalUsers = usersToProcess.Count;

      DatabaseSeedingLoggingMessages.BatchProcessed(_logger, currentBatch, totalBatches, processedUsers, totalUsers);
    }

    var validUsers = userResults
        .Where(r => r.User is not null)
        .Select(r => r!)
        .ToList(); // Materialize to avoid multiple enumeration

    var personUserMap = validUsers.ToDictionary(
      r => r.PersonId!.Value,
      r => r.User!.Id.Value);

    foreach (var (personId, userId) in personUserMap)
    {
      var person = await _draftsDbContext.People
        .Include(p => p.DrafterProfile)
        .Include(p => p.HostProfile)
        .FirstOrDefaultAsync(p => p.Id == PersonId.Create(personId), cancellationToken);

      if (person is null)
      {
        continue;
      }

      person.AssignUserId(userId);

      var user = validUsers
        .FirstOrDefault(r => r.User!.Id.Value == userId).User;

      var roleLookup = await _dbContext
        .Set<Role>()
        .ToDictionaryAsync(r => r.Name, r => r, cancellationToken);

      user!.ClearRoles();
      user!.AddRole(roleLookup["Guest"]);

      if (await _draftsDbContext.Drafters.AnyAsync(d => d.PersonId == person.Id, cancellationToken))
      {
        user!.AddRole(roleLookup["Drafter"]);
      }

      if (await _draftsDbContext.Hosts.AnyAsync(h => h.PersonId == person.Id, cancellationToken))
      {
        user!.AddRole(roleLookup["Host"]);
      }
    }

    _dbContext.Users.AddRange(validUsers.Select(r => r.User)!);

    await _draftsDbContext.SaveChangesAsync(cancellationToken);
    await SaveAndLogAsync(TableName, validUsers.Count);
  }
}
