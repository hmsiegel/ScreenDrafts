using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ScreenDrafts.Modules.Audit.Infrastructure.Mongo;

internal sealed class MongoAuditRepository
{
  private const string DatabaseName = "screendrafts_audit";

  private readonly IMongoCollection<HttpAuditLogDocument> _httpAuditLogs;
  private readonly IMongoCollection<DomainEventAuditLogDocument> _domainEventAuditLogs;
  private readonly IMongoCollection<AuthAuditLogDocument> _authAuditLogs;

  static MongoAuditRepository()
  {
    BsonSerializer.TryRegisterSerializer(new DateTimeOffsetSerializer(BsonType.String));
  }

  public MongoAuditRepository(IMongoClient mongoClient)
  {
    var database = mongoClient.GetDatabase(DatabaseName);
    _httpAuditLogs = database.GetCollection<HttpAuditLogDocument>("http_audit_logs");
    _domainEventAuditLogs = database.GetCollection<DomainEventAuditLogDocument>("domain_event_audit_logs");
    _authAuditLogs = database.GetCollection<AuthAuditLogDocument>("auth_audit_logs");
  }

  public async Task WriteHttpLogAsync(HttpAuditLog log, CancellationToken cancellationToken = default)
  {
    var document = new HttpAuditLogDocument
    {
      Id = log.Id,
      CorrelationId = log.CorrelationId,
      OccurredOnUtc = log.OccurredOnUtc,
      ActorId = log.ActorId,
      EndpointName = log.EndpointName,
      HttpMethod = log.HttpMethod,
      Route = log.Route,
      StatusCode = log.StatusCode,
      DurationMs = log.DurationMs,
      RequestBody = log.RequestBody is not null ? BsonDocument.Parse(log.RequestBody) : null,
      ResponseBody = log.ResponseBody != null ? BsonDocument.Parse(log.ResponseBody) : null,
      IpAddress = log.IpAddress
    };

    await _httpAuditLogs.InsertOneAsync(document, cancellationToken: cancellationToken);
  }

  public async Task WriteDomainEventLogAsync(DomainEventAuditLog log, CancellationToken cancellationToken = default)
  {
    var document = new DomainEventAuditLogDocument
    {
      Id = log.Id,
      OccurredOnUtc = log.OccurredOnUtc,
      EventType = log.EventType,
      SourceModule = log.SourceModule,
      ActorId = log.ActorId,
      EntityId = log.EntityId,
      Payload = BsonDocument.Parse(log.Payload)
    };
    await _domainEventAuditLogs.InsertOneAsync(document, cancellationToken: cancellationToken);
  }

  public async Task WriteAuthLogAsync(AuthAuditLog log, CancellationToken cancellationToken = default)
  {
    var document = new AuthAuditLogDocument
    {
      Id = log.Id,
      OccurredOnUtc = log.OccurredOnUtc,
      EventType = log.EventType,
      UserId = log.UserId,
      ClientId = log.ClientId,
      IpAddress = log.IpAddress,
      Details = log.Details != null ? BsonDocument.Parse(log.Details) : null
    };
    await _authAuditLogs.InsertOneAsync(document, cancellationToken: cancellationToken);
  }


  // -------------------------------------------------------------------------
  // Private document types
  // -------------------------------------------------------------------------

  private sealed class HttpAuditLogDocument
  {
    [BsonId]
    public Guid Id { get; init; }
    public Guid CorrelationId { get; init; }
    public DateTimeOffset OccurredOnUtc { get; init; }
    public string? ActorId { get; init; }
    public string EndpointName { get; init; } = string.Empty;
    public string HttpMethod { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public int? StatusCode { get; init; }
    public int? DurationMs { get; init; }

    [BsonIgnoreIfNull]
    public BsonDocument? RequestBody { get; init; }

    [BsonIgnoreIfNull]
    public BsonDocument? ResponseBody { get; init; }
    public string? IpAddress { get; init; }
  }

  private sealed class DomainEventAuditLogDocument
  {
    [BsonId]
    public Guid Id { get; init; }
    public DateTimeOffset OccurredOnUtc { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string SourceModule { get; init; } = string.Empty;
    public string? ActorId { get; init; }
    public string? EntityId { get; init; }
    public BsonDocument Payload { get; init; } = [];
  }

  private sealed class AuthAuditLogDocument
  {
    [BsonId]
    public Guid Id { get; init; }
    public DateTimeOffset OccurredOnUtc { get; init; }
    public string EventType { get; init; } = string.Empty;
    public string? UserId { get; init; }
    public string? ClientId { get; init; }
    public string? IpAddress { get; init; }

    [BsonIgnoreIfNull]
    public BsonDocument? Details { get; init; }
  }
}
