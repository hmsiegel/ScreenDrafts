namespace ScreenDrafts.Modules.Drafts.UnitTests.DraftParts.Entities;

public class CandidateListEntryTests : DraftsBaseTest
{
  // ========================================
  // Create Tests
  // ========================================

  [Fact]
  public void Create_ShouldSucceed_WhenMovieIdIsNull()
  {
    var draftPart = CreateDraftPart();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    var addedBy = "u_" + Faker.Random.AlphaNumeric(17);

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: tmdbId,
      movieId: null,
      addedByPublicId: addedBy);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Create_ShouldBePending_WhenMovieIdIsNull()
  {
    var draftPart = CreateDraftPart();

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    result.Value.IsPending.Should().BeTrue();
    result.Value.MovieId.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldNotBePending_WhenMovieIdIsProvided()
  {
    var draftPart = CreateDraftPart();
    var movieId = Guid.NewGuid();

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: movieId,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    result.Value.IsPending.Should().BeFalse();
    result.Value.MovieId.Should().Be(movieId);
  }

  [Fact]
  public void Create_ShouldSetAllProperties()
  {
    var draftPart = CreateDraftPart();
    var tmdbId = Faker.Random.Int(1, 1_000_000);
    var movieId = Guid.NewGuid();
    var addedBy = "u_" + Faker.Random.AlphaNumeric(17);
    var notes = Faker.Lorem.Sentence();

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: tmdbId,
      movieId: movieId,
      addedByPublicId: addedBy,
      notes: notes);

    var entry = result.Value;
    entry.DraftPartId.Should().Be(draftPart.Id);
    entry.DraftPartPublicId.Should().Be(draftPart.PublicId);
    entry.TmdbId.Should().Be(tmdbId);
    entry.MovieId.Should().Be(movieId);
    entry.AddedByPublicId.Should().Be(addedBy);
    entry.Notes.Should().Be(notes);
  }

  [Fact]
  public void Create_ShouldHaveNullNotes_WhenNotProvided()
  {
    var draftPart = CreateDraftPart();

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    result.Value.Notes.Should().BeNull();
  }

  [Fact]
  public void Create_ShouldUseProvidedId_WhenSpecified()
  {
    var draftPart = CreateDraftPart();
    var entryId = CandidateListEntryId.CreateUnique();

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17),
      notes: null,
      id: entryId);

    result.Value.Id.Should().Be(entryId);
  }

  [Fact]
  public void Create_ShouldGenerateUniqueId_WhenNotProvided()
  {
    var draftPart = CreateDraftPart();

    var result1 = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 500_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    var result2 = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(500_001, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    result1.Value.Id.Should().NotBe(result2.Value.Id);
  }

  [Fact]
  public void Create_ShouldSetCreatedOnUtc()
  {
    var draftPart = CreateDraftPart();
    var before = DateTime.UtcNow;

    var result = CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17));

    result.Value.CreatedOnUtc.Should().BeOnOrAfter(before);
  }

  // ========================================
  // Resolve Tests
  // ========================================

  [Fact]
  public void Resolve_ShouldSucceed_WhenEntryIsPending()
  {
    var entry = CreatePendingEntry();
    var movieId = Guid.NewGuid();

    var result = entry.Resolve(movieId);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public void Resolve_ShouldSetMovieId_WhenEntryIsPending()
  {
    var entry = CreatePendingEntry();
    var movieId = Guid.NewGuid();

    entry.Resolve(movieId);

    entry.MovieId.Should().Be(movieId);
  }

  [Fact]
  public void Resolve_ShouldClearPendingFlag_WhenEntryIsPending()
  {
    var entry = CreatePendingEntry();

    entry.Resolve(Guid.NewGuid());

    entry.IsPending.Should().BeFalse();
  }

  [Fact]
  public void Resolve_ShouldFail_WhenEntryAlreadyResolved()
  {
    var entry = CreatePendingEntry();
    entry.Resolve(Guid.NewGuid());

    var result = entry.Resolve(Guid.NewGuid());

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Should().Be(CandidateListErrors.EntryAlreadyResolved);
  }

  [Fact]
  public void Resolve_ShouldNotChangeMovieId_WhenAlreadyResolved()
  {
    var entry = CreatePendingEntry();
    var originalMovieId = Guid.NewGuid();
    entry.Resolve(originalMovieId);

    entry.Resolve(Guid.NewGuid());

    entry.MovieId.Should().Be(originalMovieId);
  }

  // ========================================
  // Helpers
  // ========================================

  private static CandidateListEntry CreatePendingEntry()
  {
    var draftPart = CreateDraftPart();
    return CandidateListEntry.Create(
      draftPartId: draftPart.Id,
      draftPartPublicId: draftPart.PublicId,
      tmdbId: Faker.Random.Int(1, 1_000_000),
      movieId: null,
      addedByPublicId: "u_" + Faker.Random.AlphaNumeric(17)).Value;
  }
}
