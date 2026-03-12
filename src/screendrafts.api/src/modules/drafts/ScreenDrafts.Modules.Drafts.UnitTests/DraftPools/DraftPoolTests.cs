namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftPools;

public class DraftPoolTests : DraftsBaseTest
{
  // ========================================
  // Create Tests
  // ========================================

  [Fact]
  public void Create_ShouldReturnSuccess_WhenValidParametersProvided()
  {
    var draftId = DraftId.CreateUnique();
    var publicId = Faker.Random.AlphaNumeric(10);

    var result = DraftPool.Create(draftId, publicId);

    result.IsSuccess.Should().BeTrue();
    result.Value.DraftId.Should().Be(draftId);
    result.Value.PublicId.Should().Be(publicId);
    result.Value.IsLocked.Should().BeFalse();
    result.Value.TmdbIds.Should().BeEmpty();
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsEmpty()
  {
    var draftId = DraftId.CreateUnique();

    var result = DraftPool.Create(draftId, string.Empty);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPoolErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldReturnFailure_WhenPublicIdIsWhitespace()
  {
    var draftId = DraftId.CreateUnique();

    var result = DraftPool.Create(draftId, "   ");

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPoolErrors.InvalidPublicId);
  }

  [Fact]
  public void Create_ShouldThrow_WhenDraftIdIsNull()
  {
    Action act = () => DraftPool.Create(null!, Faker.Random.AlphaNumeric(10));

    act.Should().Throw<ArgumentNullException>();
  }

  // ========================================
  // AddMovie Tests
  // ========================================

  [Fact]
  public void AddMovie_ShouldSucceed_WhenPoolIsUnlocked()
  {
    var pool = CreatePool();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = pool.AddMovie(tmdbId);

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().Contain(x => x.TmdbId == tmdbId);
  }

  [Fact]
  public void AddMovie_ShouldFail_WhenPoolIsLocked()
  {
    var pool = CreatePool();
    pool.Lock();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = pool.AddMovie(tmdbId);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPoolErrors.PoolIsLocked);
  }

  [Fact]
  public void AddMovie_ShouldFail_WhenMovieAlreadyExists()
  {
    var pool = CreatePool();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    pool.AddMovie(tmdbId);

    var result = pool.AddMovie(tmdbId);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPoolErrors.MovieAlreadyExists(tmdbId));
  }

  [Fact]
  public void AddMovie_ShouldTrackMultipleMovies()
  {
    var pool = CreatePool();

    pool.AddMovie(100);
    pool.AddMovie(200);
    pool.AddMovie(300);

    pool.TmdbIds.Should().HaveCount(3);
    pool.TmdbIds.Select(x => x.TmdbId).Should().BeEquivalentTo([100, 200, 300]);
  }

  // ========================================
  // RemoveMovie Tests
  // ========================================

  [Fact]
  public void RemoveMovie_ShouldRemoveMovie_WhenMovieExists()
  {
    var pool = CreatePool();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    pool.AddMovie(tmdbId);

    var result = pool.RemoveMovie(tmdbId);

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().NotContain(x => x.TmdbId == tmdbId);
  }

  [Fact]
  public void RemoveMovie_ShouldSucceed_WhenMovieNotInPool()
  {
    var pool = CreatePool();

    var result = pool.RemoveMovie(Faker.Random.Int(1, 1_000_000));

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().BeEmpty();
  }

  [Fact]
  public void RemoveMovie_ShouldOnlyRemoveTargetMovie()
  {
    var pool = CreatePool();
    pool.AddMovie(100);
    pool.AddMovie(200);

    pool.RemoveMovie(100);

    pool.TmdbIds.Should().ContainSingle(x => x.TmdbId == 200);
  }

  // ========================================
  // RestoreMovie Tests
  // ========================================

  [Fact]
  public void RestoreMovie_ShouldAddMovie_WhenNotAlreadyInPool()
  {
    var pool = CreatePool();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = pool.RestoreMovie(tmdbId);

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().Contain(x => x.TmdbId == tmdbId);
  }

  [Fact]
  public void RestoreMovie_ShouldBeIdempotent_WhenMovieAlreadyInPool()
  {
    var pool = CreatePool();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    pool.AddMovie(tmdbId);

    var result = pool.RestoreMovie(tmdbId);

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().ContainSingle(x => x.TmdbId == tmdbId);
  }

  [Fact]
  public void RestoreMovie_ShouldSucceed_EvenWhenPoolIsLocked()
  {
    var pool = CreatePool();
    pool.Lock();
    var tmdbId = Faker.Random.Int(1, 1_000_000);

    var result = pool.RestoreMovie(tmdbId);

    result.IsSuccess.Should().BeTrue();
    pool.TmdbIds.Should().Contain(x => x.TmdbId == tmdbId);
  }

  // ========================================
  // Lock Tests
  // ========================================

  [Fact]
  public void Lock_ShouldLockPool()
  {
    var pool = CreatePool();

    var result = pool.Lock();

    result.IsSuccess.Should().BeTrue();
    pool.IsLocked.Should().BeTrue();
  }

  [Fact]
  public void Lock_ShouldBeIdempotent_WhenAlreadyLocked()
  {
    var pool = CreatePool();
    pool.Lock();

    var result = pool.Lock();

    result.IsSuccess.Should().BeTrue();
    pool.IsLocked.Should().BeTrue();
  }

  [Fact]
  public void Lock_ShouldPreventFurtherMovieAddition()
  {
    var pool = CreatePool();
    pool.AddMovie(100);
    pool.Lock();

    var result = pool.AddMovie(200);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(DraftPoolErrors.PoolIsLocked);
    pool.TmdbIds.Should().ContainSingle();
  }

  // ========================================
  // Helpers
  // ========================================

  private static DraftPool CreatePool() =>
    DraftPool.Create(DraftId.CreateUnique(), Faker.Random.AlphaNumeric(10)).Value;
}
