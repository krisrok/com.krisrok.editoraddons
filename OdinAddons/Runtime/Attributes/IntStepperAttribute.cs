using System;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif

namespace OdinAddons
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IntStepperAttribute : Attribute
    { }
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public class IntstepperAttributeDrawer : OdinAttributeDrawer<IntStepperAttribute>
    {
        private static GUIContent _decreaseGuiContent;
        private static GUIContent _increaseGuiContent;

        protected override void Initialize()
        {
            base.Initialize();

            _decreaseGuiContent = new GUIContent("-");
            _increaseGuiContent = new GUIContent("+");
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            CallNextDrawer(label);
            EditorGUILayout.EndVertical();

            DrawButton(_decreaseGuiContent, () => ModifyProperty(-1));
            DrawButton(_increaseGuiContent, () => ModifyProperty(+1));

            EditorGUILayout.EndHorizontal();
        }

        private void ModifyProperty(int delta)
        {
            Property.ValueEntry.WeakSmartValue = (int)Property.ValueEntry.WeakSmartValue + delta;
        }

        private void DrawButton(GUIContent guiContent, Action action)
        {
            if (GUILayout.Button(guiContent, EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.MinWidth(20)))
            {
                Property.RecordForUndo("Click " + guiContent);

                action();
            }
        }
    }
}
#endif