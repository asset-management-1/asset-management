namespace Haven.Shared.Dtos.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class QuartzJobKeyAttribute : Attribute
{
    public QuartzJobKeyAttribute(string key) => Key = key;
    
    public string Key { get; }
}