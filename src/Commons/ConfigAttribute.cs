#pragma warning disable CS8618

namespace Dache.Common;

[AttributeUsage(AttributeTargets.Method)]
public class ConfigAttribute : Attribute
{
    /// <summary>
    /// Storage time in minutes .
    /// </summary>
    public int TimeOut { get; set; }
    
    /// <summary>
    /// The stored name of the entity inside redis .
    /// </summary>
    public string Key  { get; set; }
}