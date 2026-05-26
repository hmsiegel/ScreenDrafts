using DomainPerson = ScreenDrafts.Modules.Drafts.Domain.People.Person;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class UpdateProfileTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccess_WhenPersonExistsAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();

    var result = await Sender.Send(new UpdateProfileCommand
    {
      PublicId = publicId,
      DisplayName = Faker.Name.FullName(),
      Biography = Faker.Lorem.Paragraph(),
      Location = Faker.Address.City()
    }, TestContext.Current.CancellationToken);

    result.IsSuccess.Should().BeTrue();
  }

  [Fact]
  public async Task Should_PersistChanges_WhenPersonExistsAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var displayName = "Updated Display Name";
    var biography = "A test biography.";
    var location = "Test City";

    await Sender.Send(new UpdateProfileCommand
    {
      PublicId = publicId,
      DisplayName = displayName,
      Biography = biography,
      Location = location
    }, TestContext.Current.CancellationToken);

    var person = await DbContext.Set<DomainPerson>()
      .FirstAsync(p => p.PublicId == publicId, TestContext.Current.CancellationToken);

    person.DisplayName.Should().Be(displayName);
    person.Biography.Should().Be(biography);
    person.Location.Should().Be(location);
  }

  [Fact]
  public async Task Should_ReturnNotFound_WhenPersonDoesNotExistAsync()
  {
    var publicId = "pe_doesnotexist12";

    var result = await Sender.Send(new UpdateProfileCommand
    {
      PublicId = publicId
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
    var result = await Sender.Send(new UpdateProfileCommand
    {
      PublicId = publicId
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }

  [Fact]
  public async Task Should_ReturnValidationError_WhenDisplayNameIsTooLongAsync()
  {
    var result = await Sender.Send(new UpdateProfileCommand
    {
      PublicId = "pe_doesnotexist12",
      DisplayName = new string('x', 101)
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }
}
