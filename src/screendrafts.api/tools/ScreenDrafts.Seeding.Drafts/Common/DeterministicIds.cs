namespace ScreenDrafts.Seeding.Drafts.Common;

internal static class DeterministicIds
{
  private static readonly Guid _namespaceGuid = Guid.Parse("2c1d7d0b-7c7a-4c34-8b2b-8c1d2ff0b2b1");

  public static Guid GameBoardIdFromDraftPartId(Guid draftPartId, int partIndex = 1)
    => CreateUuidV5(_namespaceGuid, $"gameboard:{draftPartId:D}:slot:{partIndex}");

  public static Guid PickIdFromDraftPart(Guid draftPartId, int playOrder)
    => CreateUuidV5(_namespaceGuid, $"pick:{draftPartId:D}:playorder:{playOrder}");

  public static Guid VetoIdFrom(Guid targetPickId, Guid issuedByParticipantId)
    => CreateUuidV5(_namespaceGuid, $"veto:pick:{targetPickId:D}:by:{issuedByParticipantId:D}");

  public static Guid VetoOverridesFrom(Guid vetoId)
    => CreateUuidV5(_namespaceGuid, $"veto_override:veto:{vetoId:D}");

  public static Guid CommissionerOverrideIdFrom(Guid pickId)
    => CreateUuidV5(_namespaceGuid, $"commissioner_override:pick:{pickId:D}");

  private static Guid CreateUuidV5(Guid namespaceId, string name)
  {
    Span<byte> ns = stackalloc byte[16];
    namespaceId.TryWriteBytes(ns);
    SwapByteOrder(ns);

    var nameBytes = Encoding.UTF8.GetBytes(name);

    Span<byte> data = stackalloc byte[16 + nameBytes.Length];
    ns.CopyTo(data);
    nameBytes.CopyTo(data[16..]);

    Span<byte> hash = stackalloc byte[32];
    SHA256.HashData(data, hash);

    Span<byte> guidBytes = stackalloc byte[16];
    hash[..16].CopyTo(guidBytes);

    guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | (5 << 4)); // Set version to 5
    guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // Set variant to RFC 4122

    SwapByteOrder(guidBytes);
    return new Guid(guidBytes);
  }

  private static void SwapByteOrder(Span<byte> guid)
  {
    static void Swap(Span<byte> b, int i, int j) => (b[i], b[j]) = (b[j], b[i]);
    Swap(guid, 0, 3);
    Swap(guid, 1, 2);
    Swap(guid, 4, 5);
    Swap(guid, 6, 7);
  }
}
