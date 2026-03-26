using Ardalis.SmartEnum;

namespace ScreenDrafts.Modules.Drafts.Infrastructure.Data;

public sealed class SmartEnumTypeHandler<TEnum> : SqlMapper.TypeHandler<TEnum>
  where TEnum : SmartEnum<TEnum>
{
  public override TEnum? Parse(object value)
  {
    ArgumentNullException.ThrowIfNull(value);
    return SmartEnum<TEnum>.FromValue(Convert.ToInt32(value, CultureInfo.InvariantCulture));
  }

  public override void SetValue(IDbDataParameter parameter, TEnum? value)
  {
    ArgumentNullException.ThrowIfNull(parameter);
    ArgumentNullException.ThrowIfNull(value);
    parameter.Value = value.Value;
  }
}
