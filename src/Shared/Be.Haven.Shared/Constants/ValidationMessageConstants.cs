namespace Be.Haven.Shared.Constants;

public static class ValidationMessage
{
    /// <summary>
    /// Generic “required” message. Use when a value is missing or null.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string REQUIRED = "{PropertyName} is required.";

    /// <summary>
    /// Value must not equal its type’s default (e.g., default(DateTime), Guid.Empty).
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string NOT_DEFAULT = "{PropertyName} must not be the default value.";

    /// <summary>
    /// String length must be ≤ the configured maximum.
    /// Placeholders: {PropertyName}, {MaxLength}.
    /// </summary>
    public const string MAX_LENGTH = "{PropertyName} must be at most {MaxLength} characters.";

    /// <summary>
    /// String length must be ≥ the configured minimum.
    /// Placeholders: {PropertyName}, {MinLength}.
    /// </summary>
    public const string MIN_LENGTH = "{PropertyName} must be at least {MinLength} characters.";

    /// <summary>
    /// String length must be within the [min, max] bounds.
    /// Placeholders: {PropertyName}, {MinLength}, {MaxLength}.
    /// </summary>
    public const string LENGTH_BETWEEN = "{PropertyName} length must be between {MinLength} and {MaxLength}.";

    /// <summary>
    /// Value must be a syntactically valid email address.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string INVALID_EMAIL = "{PropertyName} is not a valid email.";

    /// <summary>
    /// Value must be a well-formed absolute URL (includes the scheme).
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string INVALID_ABSOLUTE_URL = "{PropertyName} must be a valid absolute URL.";

    /// <summary>
    /// Value does not match the expected format (e.g., regex).
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string INVALID_FORMAT = "{PropertyName} has an invalid format.";

    /// <summary>
    /// Collection must contain at least one element.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string COLLECTION_MUST_CONTAIN_ITEM = "{PropertyName} must contain at least one item.";
    
    /// <summary>
    /// Enum value is not defined in its type.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string ENUM_INVALID = "{PropertyName} has an invalid value.";

    /// <summary>
    /// Left property must be greater than the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string GREATER_THAN_PROP = "{PropertyName} must be greater than {ComparisonProperty}.";

    /// <summary>
    /// Left property must be greater than or equal to the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string GREATER_OR_EQUAL_PROP = "{PropertyName} must be greater than or equal to {ComparisonProperty}.";

    /// <summary>
    /// Left property must be less than the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string LESS_THAN_PROP = "{PropertyName} must be less than {ComparisonProperty}.";

    /// <summary>
    /// Left property must be less than or equal to the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string LESS_OR_EQUAL_PROP = "{PropertyName} must be less than or equal to {ComparisonProperty}.";

    /// <summary>
    /// Value must lie within [From, To] (inclusive).
    /// Placeholders: {PropertyName}, {From}, {To}.
    /// </summary>
    public const string BETWEEN_INCLUSIVE = "{PropertyName} must be between {From} and {To} (inclusive).";

    /// <summary>
    /// Value must lie within (From, To) (exclusive).
    /// Placeholders: {PropertyName}, {From}, {To}.
    /// </summary>
    public const string BETWEEN_EXCLUSIVE = "{PropertyName} must be between {From} and {To} (exclusive).";

    /// <summary>
    /// Numeric value must be greater than zero.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string GREATER_THAN_ZERO = "{PropertyName} must be greater than 0.";

    /// <summary>
    /// Numeric value must be greater than or equal to zero.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string GREATER_OR_EQUAL_ZERO = "{PropertyName} must be greater than or equal to 0.";

    /// <summary>
    /// Numeric value must be less than zero.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string LESS_THAN_ZERO = "{PropertyName} must be less than 0.";

    /// <summary>
    /// Numeric value must be less than or equal to zero.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string LESS_OR_EQUAL_ZERO = "{PropertyName} must be less than or equal to 0.";

    /// <summary>
    /// Numeric value must be greater than the specified comparison value.
    /// Placeholders: {PropertyName}, {ComparisonValue}.
    /// </summary>
    public const string GREATER_THAN_VALUE = "{PropertyName} must be greater than {ComparisonValue}.";

    /// <summary>
    /// Numeric value must be greater than or equal to the specified comparison value.
    /// Placeholders: {PropertyName}, {ComparisonValue}.
    /// </summary>
    public const string GREATER_OR_EQUAL_VALUE = "{PropertyName} must be greater than or equal to {ComparisonValue}.";

    /// <summary>
    /// Numeric value must be less than the specified comparison value.
    /// Placeholders: {PropertyName}, {ComparisonValue}.
    /// </summary>
    public const string LESS_THAN_VALUE = "{PropertyName} must be less than {ComparisonValue}.";

    /// <summary>
    /// Numeric value must be less than or equal to the specified comparison value.
    /// Placeholders: {PropertyName}, {ComparisonValue}.
    /// </summary>
    public const string LESS_OR_EQUAL_VALUE = "{PropertyName} must be less than or equal to {ComparisonValue}.";

    /// <summary>
    /// Left date/time must be earlier than the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_BEFORE = "{PropertyName} must be earlier than {ComparisonProperty}.";

    /// <summary>
    /// Left date/time must be earlier than or equal to the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_BEFORE_OR_EQUAL = "{PropertyName} must be earlier than or equal to {ComparisonProperty}.";

    /// <summary>
    /// Left date/time must be later than the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_AFTER = "{PropertyName} must be later than {ComparisonProperty}.";

    /// <summary>
    /// Left date/time must be later than or equal to the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_AFTER_OR_EQUAL = "{PropertyName} must be later than or equal to {ComparisonProperty}.";

    /// <summary>
    /// Left date/time must be equal to the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_EQUAL = "{PropertyName} must be equal to {ComparisonProperty}.";

    /// <summary>
    /// Left date/time must be different from the comparison property.
    /// Placeholders: {PropertyName}, {ComparisonProperty}.
    /// </summary>
    public const string DATE_NOT_EQUAL = "{PropertyName} must not be equal to {ComparisonProperty}.";

    /// <summary>
    /// Message indicating that a date value must be greater than the current date.
    /// Placeholder: {PropertyName}.
    /// </summary>
    public const string DATE_AFTER_NOW = "{PropertyName} must be greater than the current date.";
    
    /// <summary>
    /// Guard text used when a range rule is misconfigured: min must be less than or equal to max.
    /// </summary>
    public const string RANGE_GUARD_MIN_LE_MAX = "min must be <= max";

    /// <summary>
    /// Guard text used when a range rule is misconfigured: min must be strictly less than max.
    /// </summary>
    public const string RANGE_GUARD_MIN_LT_MAX = "min must be < max";
}
