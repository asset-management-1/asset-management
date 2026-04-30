namespace Be.Haven.Core.Extensions.Validations;

/// <summary>
/// Provides a set of validation rules as extensions to perform common validations.
/// </summary>
public static class BaseValidationRule
{
    // =====================================================================
    // Required
    // =====================================================================

    /// <summary>
    /// Validates that a string is present and not whitespace.
    /// Performs <c>NotEmpty()</c>, then <c>!string.IsNullOrWhiteSpace</c>.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> Required<T>(
        this IRuleBuilder<T, string> r,
        string customMessage = ValidationMessage.REQUIRED)
    {
        return r.NotEmpty()
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage(customMessage);
    }

    /// <summary>Validates a reference-type property is not null.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The reference property type.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp> Required<T, TProp>(
        this IRuleBuilder<T, TProp> r,
        string customMessage = ValidationMessage.REQUIRED)
        where TProp : class =>
        r.NotNull().WithMessage(customMessage);

    /// <summary>Validates a nullable value-type property is not null.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The value type of the property.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp?> Required<T, TProp>(
        this IRuleBuilder<T, TProp?> r,
        string customMessage = ValidationMessage.REQUIRED)
        where TProp : struct =>
        r.NotNull().WithMessage(customMessage);

    /// <summary>Validates a <see cref="Guid"/> property is not <see cref="Guid.Empty"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, Guid> RequiredGuid<T>(
        this IRuleBuilder<T, Guid> r,
        string customMessage = ValidationMessage.REQUIRED) =>
        r.Must(g => g != Guid.Empty).WithMessage(customMessage);

    /// <summary>Validates a value-type property is not its default value.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TStruct">The value type of the property.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TStruct> NotDefault<T, TStruct>(
        this IRuleBuilder<T, TStruct> r,
        string customMessage = ValidationMessage.NOT_DEFAULT)
        where TStruct : struct =>
        r.Must(v => !v.Equals(default(TStruct))).WithMessage(customMessage);

    // =====================================================================
    // String helpers
    // =====================================================================

    /// <summary>Validates a string has length ≤ <paramref name="max"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="max">The maximum allowed length.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> MaxLen<T>(
        this IRuleBuilder<T, string> r, int max,
        string customMessage = ValidationMessage.MAX_LENGTH) =>
        r.MaximumLength(max).WithMessage(customMessage);

    /// <summary>Validates a string has length ≥ <paramref name="min"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="min">The minimum required length.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> MinLen<T>(
        this IRuleBuilder<T, string> r, int min,
        string customMessage = ValidationMessage.MIN_LENGTH) =>
        r.MinimumLength(min).WithMessage(customMessage);

    /// <summary>Validates a string length lies within <c>[</c><paramref name="min"/>, <paramref name="max"/><c>]</c>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="min">The minimum length (inclusive).</param>
    /// <param name="max">The maximum length (inclusive).</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> BetweenLen<T>(
        this IRuleBuilder<T, string> r, int min, int max,
        string customMessage = ValidationMessage.LENGTH_BETWEEN) =>
        r.Length(min, max).WithMessage(customMessage);

    /// <summary>Validates a string is a syntactically valid email address.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> EmailFormat<T>(
        this IRuleBuilder<T, string> r,
        string customMessage = ValidationMessage.INVALID_EMAIL) =>
        r.EmailAddress().WithMessage(customMessage);

    /// <summary>Validates a string is a well-formed absolute URL.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> AbsoluteUrl<T>(
        this IRuleBuilder<T, string> r,
        string customMessage = ValidationMessage.INVALID_ABSOLUTE_URL) =>
        r.Must(u => Uri.IsWellFormedUriString(u, UriKind.Absolute)).WithMessage(customMessage);

    /// <summary>
    /// Validates that a string matches the specified regular expression pattern.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target string property.</param>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <param name="customMessage">The error message used when the rule fails. Defaults to the invalid format message.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> MatchesRegex<T>(
        this IRuleBuilder<T, string> r,
        string pattern,
        string customMessage = ValidationMessage.INVALID_FORMAT)
    {
        if (string.IsNullOrEmpty(pattern))
            return r.Must(_ => false).WithMessage(customMessage);

        return r.Must(s => s != null &&
                           Regex.IsMatch(s, pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(4)))
                .WithMessage(customMessage);
    }

    // =====================================================================
    // Collections
    // =====================================================================

    /// <summary>Validates that a collection is not null and contains at least one element.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TItem">The collection's element type.</typeparam>
    /// <param name="r">The rule builder for the target collection property.</param>
    /// <param name="requiredMessage">The message used when the collection is null.</param>
    /// <param name="emptyMessage">The message used when the collection is empty.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, IEnumerable<TItem>> NotEmptyCollection<T, TItem>(
        this IRuleBuilder<T, IEnumerable<TItem>> r,
        string requiredMessage = ValidationMessage.REQUIRED,
        string emptyMessage = ValidationMessage.COLLECTION_MUST_CONTAIN_ITEM)
    {
        return r.NotNull().WithMessage(requiredMessage)
                .Must(c => c is not null && c.Any())
                .WithMessage(emptyMessage);
    }

    // =====================================================================
    // Enums
    // =====================================================================

    /// <summary>Validates that an enum value is defined within its enumeration.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <param name="r">The rule builder for the target enum property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TEnum> ValidEnum<T, TEnum>(
        this IRuleBuilder<T, TEnum> r,
        string customMessage = ValidationMessage.ENUM_INVALID)
        where TEnum : struct, Enum =>
        r.IsInEnum().WithMessage(customMessage);

    /// <summary>
    /// Validates that a nullable enum value (TEnum?) is either null or defined in the enum.
    /// </summary>
    public static IRuleBuilderOptions<T, TEnum?> ValidEnum<T, TEnum>(
        this IRuleBuilder<T, TEnum?> ruleBuilder,
        string customMessage = ValidationMessage.ENUM_INVALID)
        where TEnum : struct, Enum =>
        ruleBuilder
            .Must(value => !value.HasValue || Enum.IsDefined(typeof(TEnum), value.Value))
            .WithMessage(customMessage);

    /// <summary>
    /// Validates that a string represents a comma-separated list of enum values of a specified enum type.
    /// Ensures that all parsed values can be mapped to the given enum type.
    /// </summary>
    /// <typeparam name="T">The type of the model being validated.</typeparam>
    /// <typeparam name="TEnum">The enum type to which the CSV values must correspond.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target string property.</param>
    /// <param name="separator">The character used to separate the values in the string. Defaults to a comma (',').</param>
    /// <param name="customMessage">An optional custom error message for validation failures. Defaults to a generic message.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, string> ValidEnum<T, TEnum>(
        this IRuleBuilder<T, string> ruleBuilder,
        string separator = COMMA_SEPARATOR,
        string customMessage = null)
        where TEnum : struct, Enum
    {
        return ruleBuilder
               .Must(value =>
               {
                   if (string.IsNullOrWhiteSpace(value)) return true;

                   var values = value
                                .Split(separator, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .Where(x => x.Length > 0);

                   foreach (var item in values)
                   {
                       // accept enum names OR numeric values
                       if (!Enum.TryParse<TEnum>(item, ignoreCase: true, out var parsed))
                           return false;

                       // block numeric values that parse but are not defined in enum
                       if (!Enum.IsDefined(typeof(TEnum), parsed))
                           return false;
                   }

                   return true;
               })
               .WithMessage(customMessage ?? ValidationMessage.ENUM_INVALID);
    }
    
    // =====================================================================
    // Compare two properties (IComparable)
    // =====================================================================

    /// <summary>Validates <c>{PropertyName} &gt; {ComparisonProperty}</c>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the left-hand property.</param>
    /// <param name="other">An expression for the right-hand (comparison) property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp> GreaterThanProp<T, TProp>(
        this IRuleBuilder<T, TProp> r, Expression<Func<T, TProp>> other,
        string customMessage = ValidationMessage.GREATER_THAN_PROP)
        where TProp : IComparable<TProp>, IComparable =>
        r.GreaterThan(other).WithMessage(customMessage);

    /// <summary>Validates <c>{PropertyName} ≥ {ComparisonProperty}</c>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the left-hand property.</param>
    /// <param name="other">An expression for the right-hand (comparison) property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp> GreaterOrEqualProp<T, TProp>(
        this IRuleBuilder<T, TProp> r, Expression<Func<T, TProp>> other,
        string customMessage = ValidationMessage.GREATER_OR_EQUAL_PROP)
        where TProp : IComparable<TProp>, IComparable =>
        r.GreaterThanOrEqualTo(other).WithMessage(customMessage);

    /// <summary>Validates <c>{PropertyName} &lt; {ComparisonProperty}</c>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the left-hand property.</param>
    /// <param name="other">An expression for the right-hand (comparison) property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp> LessThanProp<T, TProp>(
        this IRuleBuilder<T, TProp> r, Expression<Func<T, TProp>> other,
        string customMessage = ValidationMessage.LESS_THAN_PROP)
        where TProp : IComparable<TProp>, IComparable =>
        r.LessThan(other).WithMessage(customMessage);

    /// <summary>Validates <c>{PropertyName} ≤ {ComparisonProperty}</c>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the left-hand property.</param>
    /// <param name="other">An expression for the right-hand (comparison) property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TProp> LessOrEqualProp<T, TProp>(
        this IRuleBuilder<T, TProp> r, Expression<Func<T, TProp>> other,
        string customMessage = ValidationMessage.LESS_OR_EQUAL_PROP)
        where TProp : IComparable<TProp>, IComparable =>
        r.LessThanOrEqualTo(other).WithMessage(customMessage);

    // =====================================================================
    // Range (IComparable)
    // =====================================================================

    /// <summary>
    /// Validates the value lies within <c>[min, max]</c>. Throws when <paramref name="min"/> &gt; <paramref name="max"/>.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="min">The inclusive lower bound.</param>
    /// <param name="max">The inclusive upper bound.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>From</c>/<c>To</c> state.</returns>
    public static IRuleBuilderOptions<T, TProp> BetweenInclusive<T, TProp>(
        this IRuleBuilder<T, TProp> r, TProp min, TProp max,
        string customMessage = ValidationMessage.BETWEEN_INCLUSIVE)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) > 0) throw new ArgumentException(ValidationMessage.RANGE_GUARD_MIN_LE_MAX);
        return r.Must(v => v is not null && v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0)
                .WithMessage(customMessage)
                .WithState(_ => new { From = min, To = max });
    }

    /// <summary>
    /// Validates the value lies within <c>(min, max)</c>. Throws when <paramref name="min"/> ≥ <paramref name="max"/>.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TProp">The comparable property type.</typeparam>
    /// <param name="r">The rule builder for the target property.</param>
    /// <param name="min">The exclusive lower bound.</param>
    /// <param name="max">The exclusive upper bound.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>From</c>/<c>To</c> state.</returns>
    public static IRuleBuilderOptions<T, TProp> BetweenExclusive<T, TProp>(
        this IRuleBuilder<T, TProp> r, TProp min, TProp max,
        string customMessage = ValidationMessage.BETWEEN_EXCLUSIVE)
        where TProp : IComparable<TProp>
    {
        if (min.CompareTo(max) >= 0) throw new ArgumentException(ValidationMessage.RANGE_GUARD_MIN_LT_MAX);
        return r.Must(v => v is not null && v.CompareTo(min) > 0 && v.CompareTo(max) < 0)
                .WithMessage(customMessage)
                .WithState(_ => new { From = min, To = max });
    }

    // =====================================================================
    // Numeric (INumber<T>) — group overloads together
    // =====================================================================

    /// <summary>Validates a numeric value is greater than zero.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum> GreaterThanZero<T, TNum>(
        this IRuleBuilder<T, TNum> r,
        string customMessage = ValidationMessage.GREATER_THAN_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v > TNum.Zero).WithMessage(customMessage);

    /// <summary>
    /// Validates a nullable numeric value is greater than zero. Use with <c>Required</c> if field must not be null.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target nullable numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum?> GreaterThanZero<T, TNum>(
        this IRuleBuilder<T, TNum?> r,
        string customMessage = ValidationMessage.GREATER_THAN_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v is not null && v.Value > TNum.Zero).WithMessage(customMessage);

    /// <summary>Validates a numeric value is greater than or equal to zero.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum> GreaterOrEqualZero<T, TNum>(
        this IRuleBuilder<T, TNum> r,
        string customMessage = ValidationMessage.GREATER_OR_EQUAL_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v >= TNum.Zero).WithMessage(customMessage);

    /// <summary>
    /// Validates a nullable numeric value is greater than or equal to zero. Use with <c>Required</c> if needed.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target nullable numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum?> GreaterOrEqualZero<T, TNum>(
        this IRuleBuilder<T, TNum?> r,
        string customMessage = ValidationMessage.GREATER_OR_EQUAL_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v is not null && v.Value >= TNum.Zero).WithMessage(customMessage);

    /// <summary>Validates a numeric value is less than zero.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum> LessThanZero<T, TNum>(
        this IRuleBuilder<T, TNum> r,
        string customMessage = ValidationMessage.LESS_THAN_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v < TNum.Zero).WithMessage(customMessage);

    /// <summary>Validates a numeric value is less than or equal to zero.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, TNum> LessOrEqualZero<T, TNum>(
        this IRuleBuilder<T, TNum> r,
        string customMessage = ValidationMessage.LESS_OR_EQUAL_ZERO)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v <= TNum.Zero).WithMessage(customMessage);

    /// <summary>Validates a numeric value is greater than the specified minimum (exclusive).</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="min">The minimum comparison value (exclusive).</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>min</c> state.</returns>
    public static IRuleBuilderOptions<T, TNum> GreaterThanValue<T, TNum>(
        this IRuleBuilder<T, TNum> r, TNum min,
        string customMessage = ValidationMessage.GREATER_THAN_VALUE)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v > min).WithMessage(customMessage).WithState(_ => min);

    /// <summary>Validates a numeric value is greater than or equal to the specified minimum (inclusive).</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="min">The minimum comparison value (inclusive).</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>min</c> state.</returns>
    public static IRuleBuilderOptions<T, TNum> GreaterOrEqualValue<T, TNum>(
        this IRuleBuilder<T, TNum> r, TNum min,
        string customMessage = ValidationMessage.GREATER_OR_EQUAL_VALUE)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v >= min).WithMessage(customMessage).WithState(_ => min);

    /// <summary>Validates a numeric value is less than the specified maximum (exclusive).</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="max">The maximum comparison value (exclusive).</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>max</c> state.</returns>
    public static IRuleBuilderOptions<T, TNum> LessThanValue<T, TNum>(
        this IRuleBuilder<T, TNum> r, TNum max,
        string customMessage = ValidationMessage.LESS_THAN_VALUE)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v < max).WithMessage(customMessage).WithState(_ => max);

    /// <summary>Validates a numeric value is less than or equal to the specified maximum (inclusive).</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <typeparam name="TNum">The numeric type implementing <see cref="INumber{TSelf}"/>.</typeparam>
    /// <param name="r">The rule builder for the target numeric property.</param>
    /// <param name="max">The maximum comparison value (inclusive).</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options, carrying <c>max</c> state.</returns>
    public static IRuleBuilderOptions<T, TNum> LessOrEqualValue<T, TNum>(
        this IRuleBuilder<T, TNum> r, TNum max,
        string customMessage = ValidationMessage.LESS_OR_EQUAL_VALUE)
        where TNum : struct, INumber<TNum> =>
        r.Must(v => v <= max).WithMessage(customMessage).WithState(_ => max);

    // =====================================================================
    // Date comparisons — overloads grouped by method name
    // =====================================================================

    // ----- Before -----

    /// <summary>Validates that <c>{PropertyName} &lt; {ComparisonProperty}</c> for <see cref="DateTime"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> Before<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_BEFORE)
    {
        var get = other.Compile();
        return r.Must((o, a) => a < get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates that <c>{PropertyName} &lt; {ComparisonProperty}</c> for nullable <see cref="DateTime"/> values.
    /// Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> Before<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_BEFORE)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value < get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates that <c>{PropertyName} &lt; {ComparisonProperty}</c> for <see cref="DateOnly"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> Before<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_BEFORE)
    {
        var get = other.Compile();
        return r.Must((o, a) => a < get(o)).WithMessage(customMessage);
    }

    // ----- BeforeOrEqual -----

    /// <summary>Validates that <c>{PropertyName} ≤ {ComparisonProperty}</c> for <see cref="DateTime"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> BeforeOrEqual<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_BEFORE_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a <= get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates that <c>{PropertyName} ≤ {ComparisonProperty}</c> for nullable <see cref="DateTime"/> values.
    /// Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> BeforeOrEqual<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_BEFORE_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value <= get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates that <c>{PropertyName} ≤ {ComparisonProperty}</c> for <see cref="DateOnly"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> BeforeOrEqual<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_BEFORE_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a <= get(o)).WithMessage(customMessage);
    }

    // ----- After -----

    /// <summary>Validates that <c>{PropertyName} &gt; {ComparisonProperty}</c> for <see cref="DateTime"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> After<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_AFTER)
    {
        var get = other.Compile();
        return r.Must((o, a) => a > get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates that <c>{PropertyName} &gt; {ComparisonProperty}</c> for nullable <see cref="DateTime"/> values.
    /// Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> After<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_AFTER)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value > get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates that <c>{PropertyName} &gt; {ComparisonProperty}</c> for <see cref="DateOnly"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> After<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_AFTER)
    {
        var get = other.Compile();
        return r.Must((o, a) => a > get(o)).WithMessage(customMessage);
    }

    // ----- AfterOrEqual -----

    /// <summary>Validates that <c>{PropertyName} ≥ {ComparisonProperty}</c> for <see cref="DateTime"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> AfterOrEqual<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_AFTER_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a >= get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates that <c>{PropertyName} ≥ {ComparisonProperty}</c> for nullable <see cref="DateTime"/> values.
    /// Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> AfterOrEqual<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_AFTER_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value >= get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates that <c>{PropertyName} ≥ {ComparisonProperty}</c> for <see cref="DateOnly"/>.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> AfterOrEqual<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_AFTER_OR_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a >= get(o)).WithMessage(customMessage);
    }

    // ----- EqualTo -----

    /// <summary>Validates equality between two <see cref="DateTime"/> properties.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> EqualTo<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a == get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates equality for nullable <see cref="DateTime"/> values. Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> EqualTo<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value == get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates equality between two <see cref="DateOnly"/> properties.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> EqualTo<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a == get(o)).WithMessage(customMessage);
    }

    // ----- NotEqualTo -----

    /// <summary>Validates inequality between two <see cref="DateTime"/> properties.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime> NotEqualTo<T>(
        this IRuleBuilder<T, DateTime> r, Expression<Func<T, DateTime>> other,
        string customMessage = ValidationMessage.DATE_NOT_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a != get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates inequality for nullable <see cref="DateTime"/> values. Both sides must be non-null to pass.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand nullable date property.</param>
    /// <param name="other">The right-hand nullable date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> NotEqualTo<T>(
        this IRuleBuilder<T, DateTime?> r, Expression<Func<T, DateTime?>> other,
        string customMessage = ValidationMessage.DATE_NOT_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a.HasValue && get(o).HasValue && a.Value != get(o)!.Value)
                .WithMessage(customMessage);
    }

    /// <summary>Validates inequality between two <see cref="DateOnly"/> properties.</summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the left-hand date property.</param>
    /// <param name="other">The right-hand date expression to compare against.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateOnly> NotEqualTo<T>(
        this IRuleBuilder<T, DateOnly> r, Expression<Func<T, DateOnly>> other,
        string customMessage = ValidationMessage.DATE_NOT_EQUAL)
    {
        var get = other.Compile();
        return r.Must((o, a) => a != get(o)).WithMessage(customMessage);
    }

    /// <summary>
    /// Validates that a nullable DateTime value occurs after the current UTC time.
    /// </summary>
    /// <typeparam name="T">The model type being validated.</typeparam>
    /// <param name="r">The rule builder for the target DateTime property.</param>
    /// <param name="customMessage">The error message used when the rule fails.</param>
    /// <returns>The configured rule builder options.</returns>
    public static IRuleBuilderOptions<T, DateTime?> AfterNow<T>(
        this IRuleBuilder<T, DateTime?> r,
        string customMessage = ValidationMessage.DATE_AFTER_NOW)
    {
        return r.Must(d => d.HasValue && d.Value.Date > DateTime.UtcNow.Date)
                .WithMessage(customMessage);
    }
}
