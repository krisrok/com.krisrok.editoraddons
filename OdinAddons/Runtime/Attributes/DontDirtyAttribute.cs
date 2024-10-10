using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace OdinAddons
{
    /// <summary>
    /// <para>Changes via inspector do not dirty the containing scene or prefab.</para>
    /// <para>Works with bool, int, float, string, Vector3, Vector2.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class DontDirtyAttribute : Attribute
    { }
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public abstract class DontDirtyAttributeDrawer<T> : OdinAttributeDrawer<DontDirtyAttribute, T>
    {
        protected override bool CanDrawAttributeValueProperty(InspectorProperty property)
        {
            return typeof(T).IsAssignableFrom(property.ValueEntry.TypeOfValue);
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            var parent = (UnityEngine.Object)Property.Parent.ValueEntry.WeakSmartValue;
            var value = (T)Property.ValueEntry.WeakSmartValue;
            var gettersetter = Property.Info.GetGetterSetter();
            gettersetter.SetValue(parent, DrawProperty(label, value));
        }

        protected abstract T DrawProperty(GUIContent label, T value);
    }

    public class DontDirtyDrawerBool : DontDirtyAttributeDrawer<bool>
    {
        protected override bool DrawProperty(GUIContent label, bool value)
            => EditorGUILayout.Toggle(label, value);
    }

    public class DontDirtyDrawerInt : DontDirtyAttributeDrawer<int>
    {
        protected override int DrawProperty(GUIContent label, int value)
            => EditorGUILayout.IntField(label, value);
    }

    public class DontDirtyDrawerFloat : DontDirtyAttributeDrawer<float>
    {
        protected override float DrawProperty(GUIContent label, float value)
            => EditorGUILayout.FloatField(label, value);
    }

    public class DontDirtyDrawerString : DontDirtyAttributeDrawer<string>
    {
        protected override string DrawProperty(GUIContent label, string value)
            => EditorGUILayout.TextField(label, value);
    }

    public class DontDirtyDrawerVector3 : DontDirtyAttributeDrawer<Vector3>
    {
        protected override Vector3 DrawProperty(GUIContent label, Vector3 value)
            => EditorGUILayout.Vector3Field(label, value);
    }

    public class DontDirtyDrawerVector2 : DontDirtyAttributeDrawer<Vector2>
    {
        protected override Vector2 DrawProperty(GUIContent label, Vector2 value)
            => EditorGUILayout.Vector2Field(label, value);
    }
}
#endif