using System.Text.RegularExpressions;
using DomainPerson = ScreenDrafts.Modules.Drafts.Domain.People.Person;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.PeopleTests;

public class UploadAvatarTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  [Fact]
  public async Task Should_ReturnSuccessWithAvatarPath_WhenPersonExistsAndFileIsValidAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var env = GetService<IWebHostEnvironment>();
    string? uploadedFilePath = null;

    try
    {
      using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

      var result = await Sender.Send(
        new UploadAvatarCommand
        {
          PublicId = publicId,
          FileStream = stream,
          FileName = "photo.jpg",
          ContentType = "image/jpeg",
        },
        TestContext.Current.CancellationToken
      );

      result.IsSuccess.Should().BeTrue();

      // Set before any further assertion so cleanup always runs.
      uploadedFilePath = Path.Combine(env.WebRootPath, "drafters", result.Value.AvatarPath);

      // Filenames are unique per upload: {publicId}-{8 hex}.jpg
      result
        .Value.AvatarPath.Should()
        .MatchRegex($@"^{Regex.Escape(publicId)}-[0-9a-f]{{8}}\.jpg$");
      File.Exists(uploadedFilePath).Should().BeTrue();
    }
    finally
    {
      if (uploadedFilePath is not null && File.Exists(uploadedFilePath))
      {
        File.Delete(uploadedFilePath);
      }
    }
  }

  [Fact]
  public async Task Should_PersistProfilePicturePath_WhenUploadSucceedsAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    var env = GetService<IWebHostEnvironment>();
    string? uploadedFilePath = null;

    try
    {
      using var stream = new MemoryStream([0x89, 0x50, 0x4E, 0x47]);

      var result = await Sender.Send(
        new UploadAvatarCommand
        {
          PublicId = publicId,
          FileStream = stream,
          FileName = "photo.png",
          ContentType = "image/png",
        },
        TestContext.Current.CancellationToken
      );

      result.IsSuccess.Should().BeTrue();

      uploadedFilePath = Path.Combine(env.WebRootPath, "drafters", result.Value.AvatarPath);

      var person = await DbContext
        .Set<DomainPerson>()
        .FirstAsync(p => p.PublicId == publicId, TestContext.Current.CancellationToken);

      // What the API reports is what got stored.
      person.ProfilePicturePath.Should().Be(result.Value.AvatarPath);
      person
        .ProfilePicturePath.Should()
        .MatchRegex($@"^{Regex.Escape(publicId)}-[0-9a-f]{{8}}\.png$");
    }
    finally
    {
      if (uploadedFilePath is not null && File.Exists(uploadedFilePath))
      {
        File.Delete(uploadedFilePath);
      }
    }
  }

  [Fact]
  public async Task Should_ReturnError_WhenContentTypeIsInvalidAsync()
  {
    var publicId = await new PeopleFactory(Sender, Faker).CreateAndSavePersonAsync();
    using var stream = new MemoryStream([0x00]);

    var result = await Sender.Send(
      new UploadAvatarCommand
      {
        PublicId = publicId,
        FileStream = stream,
        FileName = "file.txt",
        ContentType = "text/plain",
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Failure);
    result.Errors[0].Should().Be(PersonErrors.InvalidAvatarContentType);
  }

  [Fact]
  public async Task Should_ReturnNotFound_WhenPersonDoesNotExistAsync()
  {
    var publicId = "pe_doesnotexist12";
    using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);

    var result = await Sender.Send(
      new UploadAvatarCommand
      {
        PublicId = publicId,
        FileStream = stream,
        FileName = "photo.jpg",
        ContentType = "image/jpeg",
      },
      TestContext.Current.CancellationToken
    );

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

    var result = await Sender.Send(
      new UploadAvatarCommand
      {
        PublicId = publicId,
        FileStream = stream,
        FileName = "photo.jpg",
        ContentType = "image/jpeg",
      },
      TestContext.Current.CancellationToken
    );

    result.IsFailure.Should().BeTrue();
    result.Errors[0].Type.Should().Be(ErrorType.Validation);
  }
}
