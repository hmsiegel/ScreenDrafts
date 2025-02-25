namespace ScreenDrafts.Modules.Drafts.Infrastructure.Data;

internal sealed class DraftPositionsTypeHandler : SqlMapper.TypeHandler<IReadOnlyList<DraftPositionResponse>>
{
  public override IReadOnlyList<DraftPositionResponse>? Parse(object value)
  {
    ArgumentNullException.ThrowIfNull(value);

    return JsonConvert.DeserializeObject<IReadOnlyList<DraftPositionResponse>>(value.ToString()!);
  }

  public override void SetValue(IDbDataParameter parameter, IReadOnlyList<DraftPositionResponse>? value)
  {
    ArgumentNullException.ThrowIfNull(parameter);
    parameter.Value = JsonConvert.SerializeObject(value);
  }
}
