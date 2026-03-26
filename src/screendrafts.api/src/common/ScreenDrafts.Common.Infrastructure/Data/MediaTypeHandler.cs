namespace ScreenDrafts.Common.Infrastructure.Data;

public sealed class MediaTypeHandler : SqlMapper.TypeHandler<MediaType>
{
  public override MediaType? Parse(object value) =>
    MediaType.FromValue(Convert.ToInt32(value, CultureInfo.InvariantCulture));


  public override void SetValue(IDbDataParameter parameter, MediaType? value)
  {
    ArgumentNullException.ThrowIfNull(parameter);
    parameter.Value = value?.Value;
  }
}
