namespace ScreenDrafts.Modules.Drafts.Features.Extensions;

internal static class SmartEnumValidationExtensions
{
  public static IRuleBuilderOptions<T, int> MustBeSmartEnumValue<T, TEnum>(this IRuleBuilder<T, int> ruleBuilder)
    where TEnum : SmartEnum<TEnum>
  {
    return ruleBuilder.Must(value => SmartEnum<TEnum>.TryFromValue(value, out _))
      .WithMessage($"'{{PropertyName}}' must be a valid {typeof(TEnum).Name} value.");
  }

  public static IRuleBuilderOptions<T, int?> MustBeSmartEnumValueWhenPresent<T, TEnum>(this IRuleBuilder<T, int?> ruleBuilder)
    where TEnum : SmartEnum<TEnum>
  {
    return ruleBuilder.Must(v => !v.HasValue || SmartEnum<TEnum>.TryFromValue(v.Value, out _))
      .WithMessage($"'{{PropertyName}}' must be a valid {typeof(TEnum).Name} value or null.");
  }
}
