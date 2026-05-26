using DomainPerson = ScreenDrafts.Modules.Drafts.Domain.People.Person;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class UploadAvatarTests(DraftsIntegrationTestWebAppFactory factory) : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccessWithAvatarPath_WhenPersonExistsAndFileIsValidAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var env = GetService<IWebHostEnvironment>();
    var expectedFileName = $"{publicId}.jpg";
    var expectedFilePath = Path.Combine(env.WebRootPath, "drafters", expectedFileName);

    try
    {
      using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

      var result = await Sender.Send(new UploadAvatarCommand
      {
        PublicId = publicId,
        FileStream = stream,
        FileName = "photo.jpg",
        ContentType = "image/jpeg"
      }, TestContext.Current.CancellationToken);

      result.IsSuccess.Should().BeTrue();
      result.Value.AvatarPath.Should().Be(expectedFileName);
    }
    finally
    {
      if (File.Exists(expectedFilePath))
      {
        File.Delete(expectedFilePath);
      }
    }
  }

  [Fact]
  public async Task Should_PersistProfilePicturePath_WhenUploadSucceedsAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var env = GetService<IWebHostEnvironment>();
    var expectedFileName = $"{publicId}.png";
    var expectedFilePath = Path.Combine(env.WebRootPath, "drafters", expectedFileName);

    try
    {
      using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

      await Sender.Send(new UploadAvatarCommand
      {
        PublicId = publicId,
        FileStream = stream,
        FileName = "photo.png",
        ContentType = "image/png"
      }, TestContext.Current.CancellationToken);

      var person = await DbContext.Set<DomainPerson>()
        .FirstAsync(p => p.PublicId == publicId, TestContext.Current.CancellationToken);

      person.ProfilePicturePath.Should().Be(expectedFileName);
    }
    finally
    {
      if (File.Exists(expectedFilePath))
      {
        File.Delete(expectedFilePath);
      }
    }
  }

  [Fact]
  public async Task Should_ReturnError_WhenContentTypeIsInvalidAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    using var stream = new MemoryStream([0x00]);

    var result = await Sender.Send(new UploadAvatarCommand
    {
      PublicId = publicId,
      FileStream = stream,
      FileName = "file.txt",
      ContentType = "text/plain"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Failure);
    result.Errors[0].Should().Be(PersonErrors.InvalidAvatarContentType);
  }

  [Fact]
  public async Task Should_ReturnNotFound_WhenPersonDoesNotExistAsync()
  {
    var publicId = "pe_doesnotexist12";
    using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

    var result = await Sender.Send(new UploadAvatarCommand
    {
      PublicId = publicId,
      FileStream = stream,
      FileName = "photo.jpg",
      ContentType = "image/jpeg"
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
    using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

    var result = await Sender.Send(new UploadAvatarCommand
    {
      PublicId = publicId,
      FileStream = stream,
      FileName = "photo.jpg",
      ContentType = "image/jpeg"
    }, TestContext.Current.CancellationToken);

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }
}
