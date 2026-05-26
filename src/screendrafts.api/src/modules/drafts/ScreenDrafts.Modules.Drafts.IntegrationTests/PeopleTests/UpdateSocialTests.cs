using DomainPerson = ScreenDrafts.Modules.Drafts.Domain.People.Person;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class UpdateSocialTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenPersonExistsAndHandlesProvidedAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();

    var result = await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId,
      TwitterHandle = "twitteruser",
      InstagramHandle = "instagramuser",
      LetterboxdHandle = "letterboxduser",
      BlueskyHandle = "blueskyuser"
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Should_PersistHandles_WhenPersonExistsAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var twitterHandle = "twitteruser";
    var instagramHandle = "instagramuser";
    var letterboxdHandle = "letterboxduser";
    var blueskyHandle = "blueskyhandle";

    await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId,
      TwitterHandle = twitterHandle,
      InstagramHandle = instagramHandle,
      LetterboxdHandle = letterboxdHandle,
      BlueskyHandle = blueskyHandle
    }, TestContext.Current.CancellationToken);

    var person = await DbContext.Set<DomainPerson>()
      .FirstAsync(p => p.PublicId == publicId, TestContext.Current.CancellationToken);

    person.TwitterHandle.Should().Be(twitterHandle);
    person.InstagramHandle.Should().Be(instagramHandle);
    person.LetterboxdHandle.Should().Be(letterboxdHandle);
    person.BlueskyHandle.Should().Be(blueskyHandle);
  }

  [Fact]
  public async Task Should_ClearHandles_WhenNullHandlesPassedAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();

    await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId,
      TwitterHandle = "handle",
      InstagramHandle = "instahandle"
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId,
      TwitterHandle = null,
      InstagramHandle = null
    }, TestContext.Current.CancellationToken);

    var person = await DbContext.Set<DomainPerson>()
      .FirstAsync(p => p.PublicId == publicId, TestContext.Current.CancellationToken);

    person.TwitterHandle.Should().BeNull();
    person.InstagramHandle.Should().BeNull();
  }

  [Fact]
  public async Task Should_ReturnNotFound_WhenPersonDoesNotExistAsync()
  {
    var publicId = "pe_doesnotexist12";

    var result = await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId,
      TwitterHandle = "twitteruser"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.NotFound);
    result.Errors[0].Should().Be(PersonErrors.NotFound(publicId));
  }

  [Theory]
  [InlineData("")]
  [InlineData("noprefixhere")]
  [InlineData("pe_tiny")]
  public async Task Should_ReturnValidationError_WhenPublicIdIsInvalidAsync(string publicId)
  {
    var result = await Sender.Send(new UpdateSocialCommand
    {
      PublicId = publicId
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }

  [Fact]
  public async Task Should_ReturnValidationError_WhenTwitterHandleIsTooLongAsync()
  {
    var result = await Sender.Send(new UpdateSocialCommand
    {
      PublicId = "pe_doesnotexist12",
      TwitterHandle = new string('x', 16)
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }
}
