using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OdinAddons
{
    /// <summary>
    /// Attribute to select a single layer.
    /// </summary>
    public class LayerAttribute : PropertyAttribute
    { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(LayerAttribute))]
    class LayerAttributePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            property.intValue = EditorGUI.LayerField(position, label, property.intValue);
            EditorGUI.EndProperty();
        }
    }
#endif
}