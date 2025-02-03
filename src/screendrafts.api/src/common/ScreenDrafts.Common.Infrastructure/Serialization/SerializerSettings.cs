namespace ScreenDrafts.Common.Infrastructure.Serialization;
public static class SerializerSettings
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2326:Do not use TypeNameHandling values other than None", Justification = "Reviewed")]
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Security", "CA2327:Do not us insecure JsonSerializerSettings", Justification = "Reviewed")]
  public static readonly JsonSerializerSettings Instance = new()
  {
    TypeNameHandling = TypeNameHandling.All,
    MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
  };
}
