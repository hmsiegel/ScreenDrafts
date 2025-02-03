namespace ScreenDrafts.Common.Infrastructure.Data;

internal sealed class GeneciArrayHandler<T> : SqlMapper.TypeHandler<T[]>
{
  public override T[]? Parse(object value)
  {
    return value as T[];
  }
  public override void SetValue(IDbDataParameter parameter, T[]? value)
  {
    parameter.Value = value;
  }
}
