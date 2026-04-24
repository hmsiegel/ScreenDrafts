using Microsoft.Extensions.Logging;

using ScreenDrafts.Common.Application.Services;
using ScreenDrafts.Modules.Drafts.Domain.DraftParts.Repositories;
using ScreenDrafts.Modules.Drafts.Features.DraftParts.Zoom;

namespace ScreenDrafts.Modules.Drafts.IntegrationTests.DraftParts.Zoom;

public sealed class ZoomRecordingCompletedConsumerTests(DraftsIntegrationTestWebAppFactory factory)
  : DraftsIntegrationTest(factory)
{
  // -------------------------------------------------------------------------
  // Happy path — recordings persisted
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldPersistRecordings_WhenDraftPartMatchesSessionNameAsync()
  {
    // Arrange
    var (_, sessionName) = await SetupDraftPartWithSessionAsync();

    var now = DateTimeOffset.UtcNow;
    var @event = BuildEvent(sessionName, files:
    [
      BuildFile("file-001", "MP4", now.AddMinutes(-60), now.AddMinutes(-30))
    ]);

    var consumer = CreateConsumer();

    // Act
    await consumer.Handle(@event, TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    var recordings = await DbContext.DraftPartRecordings
      .Where(r => r.ZoomSessionName == sessionName)
      .ToListAsync(TestContext.Current.CancellationToken);

    recordings.Should().ContainSingle();
    recordings[0].ZoomFileId.Should().Be("file-001");
    recordings[0].FileType.Should().Be(ZoomRecordingFileType.Video);
    recordings[0].SequenceNumber.Should().Be(1);
  }

  [Fact]
  public async Task Handle_ShouldAssignSequenceNumbers_InChronologicalOrderAsync()
  {
    // Arrange
    var (_, sessionName) = await SetupDraftPartWithSessionAsync();

    var t0 = DateTimeOffset.UtcNow.AddHours(-2);
    var @event = BuildEvent(sessionName, files:
    [
      BuildFile("file-002", "MP4",  t0.AddMinutes(60), t0.AddMinutes(90)),
      BuildFile("file-001", "M4A",  t0,                t0.AddMinutes(90)),
    ]);

    var consumer = CreateConsumer();

    // Act
    await consumer.Handle(@event, TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert — recordings are ordered by RecordingStart, so M4A (t0) gets seq=1
    var recordings = await DbContext.DraftPartRecordings
      .Where(r => r.ZoomSessionName == sessionName)
      .OrderBy(r => r.SequenceNumber)
      .ToListAsync(TestContext.Current.CancellationToken);

    recordings.Should().HaveCount(2);
    recordings[0].ZoomFileId.Should().Be("file-001");
    recordings[0].SequenceNumber.Should().Be(1);
    recordings[1].ZoomFileId.Should().Be("file-002");
    recordings[1].SequenceNumber.Should().Be(2);
  }

  [Fact]
  public async Task Handle_ShouldIncrementSequenceNumbers_WhenRecordingsAlreadyExistAsync()
  {
    // Arrange
    var (_, sessionName) = await SetupDraftPartWithSessionAsync();

    var t0 = DateTimeOffset.UtcNow.AddHours(-2);

    // First batch
    var consumer = CreateConsumer();
    await consumer.Handle(
      BuildEvent(sessionName, files: [BuildFile("file-001", "MP4", t0, t0.AddMinutes(30))]),
      TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Second batch (separate Zoom recording session)
    await consumer.Handle(
      BuildEvent(sessionName, files: [BuildFile("file-002", "MP4", t0.AddHours(1), t0.AddHours(2))]),
      TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert
    var recordings = await DbContext.DraftPartRecordings
      .Where(r => r.ZoomSessionName == sessionName)
      .OrderBy(r => r.SequenceNumber)
      .ToListAsync(TestContext.Current.CancellationToken);

    recordings.Should().HaveCount(2);
    recordings[0].SequenceNumber.Should().Be(1);
    recordings[1].SequenceNumber.Should().Be(2);
  }

  // -------------------------------------------------------------------------
  // Guard — DraftPart not found for session name
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldDoNothing_WhenDraftPartNotFoundForSessionNameAsync()
  {
    // Arrange — session name that doesn't exist in DB
    var consumer = CreateConsumer();
    var @event = BuildEvent("screendrafts-nonexistent-dp", files:
    [
      BuildFile("file-001", "MP4", DateTimeOffset.UtcNow.AddHours(-1), DateTimeOffset.UtcNow)
    ]);

    // Act
    await consumer.Handle(@event, TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert — no recordings written
    var count = await DbContext.DraftPartRecordings.CountAsync(TestContext.Current.CancellationToken);
    count.Should().Be(0);
  }

  // -------------------------------------------------------------------------
  // Guard — duplicate file IDs are skipped
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSkipDuplicateFile_WhenZoomFileIdAlreadyExistsAsync()
  {
    // Arrange
    var (_, sessionName) = await SetupDraftPartWithSessionAsync();
    var consumer = CreateConsumer();
    var t0 = DateTimeOffset.UtcNow.AddHours(-1);
    var file = BuildFile("file-dup", "MP4", t0, t0.AddMinutes(30));

    // First delivery
    await consumer.Handle(BuildEvent(sessionName, files: [file]), TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Act — second delivery with the same file ID (re-delivery scenario)
    await consumer.Handle(BuildEvent(sessionName, files: [file]), TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert — only one recording stored
    var recordings = await DbContext.DraftPartRecordings
      .Where(r => r.ZoomFileId == "file-dup")
      .ToListAsync(TestContext.Current.CancellationToken);

    recordings.Should().ContainSingle();
  }

  // -------------------------------------------------------------------------
  // Guard — unrecognized file types are skipped
  // -------------------------------------------------------------------------

  [Fact]
  public async Task Handle_ShouldSkipUnrecognizedFileType_AndProcessKnownTypesAsync()
  {
    // Arrange
    var (_, sessionName) = await SetupDraftPartWithSessionAsync();
    var consumer = CreateConsumer();
    var t0 = DateTimeOffset.UtcNow.AddHours(-1);

    var @event = BuildEvent(sessionName, files:
    [
      BuildFile("file-known", "MP4",     t0, t0.AddMinutes(30)),
      BuildFile("file-unknown", "SLIDES", t0, t0.AddMinutes(30)),
    ]);

    // Act
    await consumer.Handle(@event, TestContext.Current.CancellationToken);
    await DbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

    // Assert — only the known type was stored
    var recordings = await DbContext.DraftPartRecordings
      .Where(r => r.ZoomSessionName == sessionName)
      .ToListAsync(TestContext.Current.CancellationToken);

    recordings.Should().ContainSingle();
    recordings[0].ZoomFileId.Should().Be("file-known");
  }

  // -------------------------------------------------------------------------
  // Helpers
  // -------------------------------------------------------------------------

  private async Task<(string DraftPartPublicId, string SessionName)> SetupDraftPartWithSessionAsync()
  {
    var seriesResult = await Sender.Send(new CreateSeriesCommand
    {
      Name = Faker.Company.CompanyName(),
      Kind = SeriesKind.Regular.Value,
      CanonicalPolicy = CanonicalPolicy.Always.Value,
      ContinuityScope = ContinuityScope.None.Value,
      ContinuityDateRule = ContinuityDateRule.AnyChannelFirstRelease.Value,
      AllowedDraftTypes = (int)DraftTypeMask.All,
      DefaultDraftType = DraftType.Standard.Value
    }, TestContext.Current.CancellationToken);

    var draftResult = await Sender.Send(new CreateDraftCommand
    {
      Title = Faker.Company.CompanyName(),
      DraftType = DraftType.Standard.Value,
      SeriesId = seriesResult.Value,
    }, TestContext.Current.CancellationToken);

    await Sender.Send(new CreateDraftPartCommand
    {
      DraftPublicId = draftResult.Value,
      PartIndex = 1,
      MinimumPosition = 1,
      MaximumPosition = 7,
    }, TestContext.Current.CancellationToken);

    var draftPartPublicId = await GetFirstDraftPartPublicIdAsync(draftResult.Value);
    var hostPublicId = await new HostFactory(Sender, Faker).CreateAndSaveHostAsync();

    var startResult = await Sender.Send(new StartZoomSessionCommand
    {
      DraftPartPublicId = draftPartPublicId,
      HostPublicId = hostPublicId
    }, TestContext.Current.CancellationToken);

    return (draftPartPublicId, startResult.Value.SessionName);
  }

  private ZoomRecordingCompletedIntegrationEventConsumer CreateConsumer() =>
    new(
      GetService<IDraftPartRepository>(),
      GetService<IDraftPartRecordingRepository>(),
      GetService<ILogger<ZoomRecordingCompletedIntegrationEventConsumer>>(),
      GetService<IPublicIdGenerator>());

  private static ZoomRecordingCompletedIntegrationEvent BuildEvent(
    string sessionName,
    IReadOnlyList<ZoomRecordingFileModel>? files = null) =>
    new(
      id: Guid.NewGuid(),
      occurredOnUtc: DateTime.UtcNow,
      zoomMeetingId: $"zoom-{Guid.NewGuid():N}",
      meetingTopic: sessionName,
      meetingStartTime: DateTimeOffset.UtcNow.AddHours(-1),
      meetingDurationMinutes: 60,
      recordingFiles: files ?? []);

  private static ZoomRecordingFileModel BuildFile(
    string fileId,
    string fileType,
    DateTimeOffset start,
    DateTimeOffset end) =>
    new(
      zoomFileId: fileId,
      fileType: fileType,
      playUrl: new Uri($"https://zoom.us/play/{fileId}"),
      downloadUrl: new Uri($"https://zoom.us/download/{fileId}"),
      recordingStart: start,
      recordingEnd: end,
      fileSizeBytes: 1_024_000L);
}
