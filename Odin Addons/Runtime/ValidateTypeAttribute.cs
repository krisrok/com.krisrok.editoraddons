using System;
using UnityEngine;

/// <summary>
/// <see cref="ValidateTypeAttribute"/> can be used to validate the value of the field/property is assignable to a specific type.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class ValidateTypeAttribute : Attribute
{
    public Type Type { get; }
    public string MemberName { get; }

    public bool GetLabelFromParent { get; set; }

    /// <summary>
    /// Auto-searches for a matching component if the given value does not match the requested type. Only works for <see cref="GameObject"/> and <see cref="Component"/>.
    /// <para>
    /// True by default.
    /// </para>
    /// </summary>
    public bool AutoSelectMatchingComponent { get; set; } = true;

    public ValidateTypeAttribute(Type type)
    {
        Type = type;
    }

    public ValidateTypeAttribute(string memberName)
    {
        MemberName = memberName;
    }
}
