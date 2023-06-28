using System;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

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

#if UNITY_EDITOR
public class ValidateTypeAttributeDrawer : OdinAttributeDrawer<ValidateTypeAttribute>
{
    private ValueResolver<Type> _typeGetter;
    private GUIContent _label;

    protected override void Initialize()
    {
        base.Initialize();

        _typeGetter = ValueResolver.Get<Type>(Property, Attribute.MemberName);
        _label = Attribute.GetLabelFromParent ? Property.Parent.Label : Property.Label;
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        if (IsAllGoodOrHasBeenFixed() == false)
            SirenixEditorGUI.ErrorMessageBox($"{_label} needs to be of type {GetTypeFromAttribute().Name}");

        CallNextDrawer(label);
    }

    private bool IsAllGoodOrHasBeenFixed()
    {
        if (Property.ValueEntry.ValueState == PropertyValueState.NullReference)
            return true;

        Type type = GetTypeFromAttribute();

        if (type.IsAssignableFrom(Property.ValueEntry.TypeOfValue))
            return true;

        if (Attribute.AutoSelectMatchingComponent)
        {
            if (TryAutoSelectMatchingComponentOnGameObject(type))
                return true;
        }

        return false;
    }

    private bool TryAutoSelectMatchingComponentOnGameObject(Type type)
    {
        var gameObject = Property.ValueEntry.WeakSmartValue as GameObject;
        if (gameObject == null)
            return false;

        var foundComponent = gameObject.GetComponent(type);
        if (foundComponent == null)
            return false;

        if (type.IsAssignableFrom(foundComponent.GetType()) == false)
            return false;

        Property.ValueEntry.WeakSmartValue = foundComponent;
        return true;
    }

    private Type GetTypeFromAttribute()
    {
        if (Attribute.Type != null)
            return Attribute.Type;

        return _typeGetter.GetValue();
    }
}
#endif