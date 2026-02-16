namespace ScreenDrafts.Common.Infrastructure.Data;

public sealed class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
{
  public override T? Parse(object value)
  {
    ArgumentNullException.ThrowIfNull(value);

    return JsonConvert.DeserializeObject<T>(value.ToString()!);
  }

  public override void SetValue(IDbDataParameter parameter, T? value)
  {
    ArgumentNullException.ThrowIfNull(parameter);
    parameter.Value = JsonConvert.SerializeObject(value);
  }
}
