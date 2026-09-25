namespace CsharpTestAutomation.Framework.Common.Utilities.CustomAttributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class PropertyAlias(string alias) : Attribute
{
    public string Alias { get; set; } = alias;
}
