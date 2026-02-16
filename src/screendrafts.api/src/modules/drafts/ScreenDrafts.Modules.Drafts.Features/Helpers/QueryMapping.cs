using ScreenDrafts.Modules.Drafts.Features.SeriesFeatures;
using ScreenDrafts.Modules.Drafts.Domain.SeriesAggregate.Enums;

namespace ScreenDrafts.Modules.Drafts.Features.Helpers;

internal static class QueryMapping
{
  public static SmartEnumResponse ToSmartEnumResponse<TEnum>(int value)
      where TEnum : SmartEnum<TEnum>
  {
    if (!SmartEnum<TEnum>.TryFromValue(value, out var e))
    {
      throw new ArgumentOutOfRangeException(nameof(value), $"Value '{value}' is not valid for enum '{typeof(TEnum).Name}'.");
    }
      return new SmartEnumResponse
      {
          Value = e.Value,
          Name = e.Name
      };
  }

  public static SmartEnumResponse? ToSmartEnumResponseNullable<TEnum>(int? value)
      where TEnum : SmartEnum<TEnum>
    =>  value.HasValue ? ToSmartEnumResponse<TEnum>(value.Value) : null;

  public static IReadOnlyList<SmartEnumResponse> DraftTypesFromMask(int maskValue)
  {
    var mask = (DraftTypeMask)maskValue;

    if ((mask & ~DraftTypeMask.All) != 0)
    {
      throw new InvalidOperationException($"Mask value '{maskValue}' contains invalid DraftTypeMask flags.");
    }

    return [.. DraftType.List
      .Where(dt => mask.Allows(dt))
      .Select(dt => new SmartEnumResponse { Name = dt.Name, Value = dt.Value })];
  }

  public static IReadOnlyList<SmartEnumResponse> AllSmartEnums<TEnum>()
    where TEnum : SmartEnum<TEnum>
    => [.. SmartEnum<TEnum>.List
    .Select(e => new SmartEnumResponse
    {
      Value = e.Value,
      Name = e.Name
    })];

  public static IReadOnlyList<SmartEnumResponse> AllDraftTypes()
    => [.. DraftType.List
    .Select(dt => new SmartEnumResponse
    {
      Name = dt.Name,
      Value = dt.Value
    })];

  public static SeriesResponse Map(SeriesRow row)
  {
    return new SeriesResponse
    {
      PublicId = row.PublicId,
      Name = row.Name,
      Kind = ToSmartEnumResponse<SeriesKind>(row.Kind),
      CanonicalPolicy = ToSmartEnumResponse<CanonicalPolicy>(row.CanonicalPolicy),
      ContinuityScope = ToSmartEnumResponse<ContinuityScope>(row.ContinuityScope),
      ContinuityDateRule = ToSmartEnumResponse<ContinuityDateRule>(row.ContinuityDateRule),
      AllowedDraftTypesMask = row.AllowedDraftTypesMask,
      AllowedDraftTypes = DraftTypesFromMask(row.AllowedDraftTypesMask),
      DefaultDraftType = ToSmartEnumResponseNullable<DraftType>(row.DefaultDraftType)
    };
  }
}
