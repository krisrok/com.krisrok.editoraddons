using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
#endif

namespace Sirenix.OdinInspector
{
    /// <summary>
    /// Draws the first item of a list as a one-line property.
    /// If the list is null, contains 0 or >1 elements, it draws a normal list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class CompactListAttribute : Attribute
    { }
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public class CompactListAttributeDrawer<T, TList> : OdinAttributeDrawer<CompactListAttribute, TList>
            where TList : IList<T>
            where T : UnityEngine.Object
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var list = (IList<T>)ValueEntry.SmartValue;

            if (list == null || list.Count != 1)
            {
                CallNextDrawer(label);
                return;
            }

            DrawSingleItemList(label, list);
        }

        private void DrawSingleItemList(GUIContent label, IList<T> list)
        {
            EditorGUI.BeginChangeCheck();

            var s = new GUIStyle(SirenixGUIStyles.ToggleGroupBackground);

            EditorGUILayout.BeginHorizontal(s);

            EditorGUILayout.Space(2, expand: false);

            EditorGUILayout.BeginHorizontal(EditorStyles.foldout);
            if (label != null)
            {
                label = new GUIContent(label);
                label.text += " [0]";
            }

            // for when T is open generic
            //list[0] = (T)SirenixEditorFields.PolymorphicObjectField(label, list[0], typeof(T), allowSceneObjects: true);
            list[0] = (T)EditorGUILayout.ObjectField(label, list[0], typeof(T), allowSceneObjects: true);

            EditorGUILayout.Space(2, expand: false);

            if (SirenixEditorGUI.IconButton(EditorIcons.X, 14, 22))
            {
                list.RemoveAt(0);
            }

            EditorGUILayout.Space(2, expand: false);

            SirenixEditorGUI.VerticalLineSeparator();

            EditorGUILayout.Space(2, expand: false);

            if (SirenixEditorGUI.IconButton(EditorIcons.Plus, 16, 20))
            {
                list.Add(default(T));
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
                ValueEntry.ApplyChanges();
        }
    }
}
#endif