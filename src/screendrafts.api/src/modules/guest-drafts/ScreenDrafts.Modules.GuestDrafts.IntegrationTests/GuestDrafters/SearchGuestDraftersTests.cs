namespace ScreenDrafts.Modules.GuestDrafts.IntegrationTests.GuestDrafters;

/// <summary>
/// SearchGuestDraftersQueryHandler goes through IDbConnectionFactory/Dapper
/// directly, not the repository -- IGuestDrafterRepository no longer has a
/// SearchAsync method at all.
/// </summary>
public sealed class SearchGuestDraftersTests(GuestDraftsIntegrationTestWebAppFactory factory)
  : GuestDraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Search_ByFirstNameSubstring_ShouldReturnMatchingGuestDraftersAsync()
  {
    // Arrange
    var target = await CreateGuestDrafterAsync("Zephyrine", "Okafor");
    await CreateGuestDrafterAsync("Marcus", "Aurelius");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = "Zephy" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(d => d.PublicId == target);
  }

  [Fact]
  public async Task Search_ByLastNameSubstring_ShouldReturnMatchingGuestDraftersAsync()
  {
    // Arrange
    var target = await CreateGuestDrafterAsync("Marcus", "Okonkwo");
    await CreateGuestDrafterAsync("Zephyrine", "Aurelius");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = "Okonkwo" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(d => d.PublicId == target);
  }

  [Fact]
  public async Task Search_ShouldMatchCaseInsensitivelyAsync()
  {
    // Arrange
    var target = await CreateGuestDrafterAsync("Quillfeather", "Nakamura");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = "quillFEATHER" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().ContainSingle(d => d.PublicId == target);
  }

  [Fact]
  public async Task Search_WithEmptySearch_ShouldReturnAllGuestDraftersAsync()
  {
    // Arrange
    var first = await CreateGuestDrafterAsync("Aldric", "Fennimore");
    var second = await CreateGuestDrafterAsync("Belinda", "Hawksworth");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = string.Empty },
      TestContext.Current.CancellationToken
    );

    // Assert -- don't assert an exact count, only that both seeded rows are present
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Contain(d => d.PublicId == first);
    result.Value.Should().Contain(d => d.PublicId == second);
  }

  [Fact]
  public async Task Search_WithNullSearch_ShouldReturnAllGuestDraftersAsync()
  {
    // Arrange
    var target = await CreateGuestDrafterAsync("Cordelia", "Thistlewood");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = null },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().Contain(d => d.PublicId == target);
  }

  [Fact]
  public async Task Search_WithNoMatch_ShouldReturnAnEmptyListAsync()
  {
    // Arrange
    await CreateGuestDrafterAsync("Reginald", "Ashcombe");

    // Act
    var result = await Sender.Send(
      new SearchGuestDraftersQuery { Search = "ZzzNoSuchNameZzz" },
      TestContext.Current.CancellationToken
    );

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().BeEmpty();
  }

  private async Task<string> CreateGuestDrafterAsync(string firstName, string lastName)
  {
    var result = await Sender.Send(
      new CreateGuestDrafterCommand { UserId = Guid.NewGuid(), FirstName = firstName, LastName = lastName },
      TestContext.Current.CancellationToken
    );
    result.IsSuccess.Should().BeTrue("test setup must be able to create a guest drafter");
    return result.Value;
  }
}
