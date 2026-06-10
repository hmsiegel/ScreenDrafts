namespace ScreenDrafts.Modules.Drafts.Domain.Attendances;

public sealed class DraftPartAttendance : AggregateRoot<DraftPartAttendanceId, Guid>
{
  private DraftPartAttendance(
    DraftPartAttendanceId id,
    DraftPartId draftPartId,
    string personPublicId,
    string publicId
  )
    : base(id)
  {
    DraftPartId = draftPartId;
    PersonPublicId = personPublicId;
    PublicId = publicId;
    Status = AttendanceStatus.Pending;
    CreatedAtUtc = DateTime.UtcNow;
  }

  private DraftPartAttendance() { }

  public string PublicId { get; private set; } = default!;
  public DraftPartId DraftPartId { get; private set; } = default!;
  public string PersonPublicId { get; private set; } = default!;
  public AttendanceStatus Status { get; private set; } = default!;
  public DateTime CreatedAtUtc { get; private set; }
  public DateTime? UpdatedAtUtc { get; private set; }

  public static Result<DraftPartAttendance> Create(
    DraftPartId draftPartId,
    string personPublicId,
    string publicId
  )
  {
    ArgumentNullException.ThrowIfNull(draftPartId);

    if (string.IsNullOrWhiteSpace(publicId))
      return Result.Failure<DraftPartAttendance>(AttendanceErrors.InvalidPublicId);

    if (string.IsNullOrWhiteSpace(personPublicId))
      return Result.Failure<DraftPartAttendance>(AttendanceErrors.InvalidPublicId);

    var attendance = new DraftPartAttendance(
      id: DraftPartAttendanceId.CreateUnique(),
      draftPartId: draftPartId,
      personPublicId: personPublicId,
      publicId: publicId
    );

    attendance.Raise(
      new AttendanceCreatedDomainEvent(attendance.Id.Value, draftPartId.Value, personPublicId)
    );

    return Result.Success(attendance);
  }

  /// <summary>Admin confirms the person will attend. Requires Pending status.</summary>
  public Result Confirm()
  {
    if (Status != AttendanceStatus.Pending)
      return Result.Failure(AttendanceErrors.NotPending);

    Status = AttendanceStatus.Confirmed;
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new AttendanceConfirmedDomainEvent(Id.Value, DraftPartId.Value, PersonPublicId));

    return Result.Success();
  }

  /// <summary>Person joins the draft part. Requires Confirmed status.</summary>
  public Result Join()
  {
    if (Status != AttendanceStatus.Confirmed)
      return Result.Failure(AttendanceErrors.NotConfirmed);

    Status = AttendanceStatus.Joined;
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new AttendanceJoinedDomainEvent(Id.Value, DraftPartId.Value, PersonPublicId));

    return Result.Success();
  }

  /// <summary>Person or admin withdraws. Valid from any non-Withdrawn status.</summary>
  public Result Withdraw()
  {
    if (Status == AttendanceStatus.Withdrawn)
      return Result.Failure(AttendanceErrors.AlreadyWithdrawn);

    Status = AttendanceStatus.Withdrawn;
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new AttendanceWithdrawnDomainEvent(Id.Value, DraftPartId.Value, PersonPublicId));

    return Result.Success();
  }

  /// <summary>Admin reinstates a withdrawn record back to Pending.</summary>
  public Result Reinstate()
  {
    if (Status != AttendanceStatus.Withdrawn)
      return Result.Failure(AttendanceErrors.NotWithdrawn);

    Status = AttendanceStatus.Pending;
    UpdatedAtUtc = DateTime.UtcNow;

    Raise(new AttendanceReinstatedDomainEvent(Id.Value, DraftPartId.Value, PersonPublicId));

    return Result.Success();
  }
}
