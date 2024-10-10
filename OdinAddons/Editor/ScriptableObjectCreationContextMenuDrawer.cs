using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using System;
using UnityEditor;
using UnityEngine;

namespace OdinAddons.Drawers
{
    /// <summary>
    /// Adds a context menu entry on inspector properties of type ScriptableObject to easily create a matching asset.
    /// </summary>
    public class ScriptableObjectCreationContextMenuDrawer
        : OdinValueDrawer<ScriptableObject>, IDefinesGenericMenuItems
    {
        protected override void Initialize()
        {
            // We don't use this drawer to "draw" something so skip it when drawing.
            SkipWhenDrawing = true;
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            Type type = property.ValueEntry.TypeOfValue;
            genericMenu.AddSeparator(string.Empty);
            genericMenu.AddItem(GUIHelper.TempContent($"Create new {type.Name} asset"), false, () =>
            {
                var so = ScriptableObject.CreateInstance(type);

                var defaultName = type.Name;

                var rootValue = property.SerializationRoot.ValueEntry.WeakSmartValue as UnityEngine.Object;
                if (rootValue != null)
                {
                    defaultName = $"{rootValue.name}_{type.Name}";
                }

                var path = EditorUtility.SaveFilePanelInProject("Save", defaultName, "asset", "Save asset");

                if (path.Length == 0)
                    return;

                property.ValueEntry.WeakSmartValue = so;
                property.MarkSerializationRootDirty();

                AssetDatabase.CreateAsset(so, path);
                AssetDatabase.SaveAssets();

                EditorUtility.FocusProjectWindow();
            });
            genericMenu.AddSeparator(string.Empty);
        }
    }
}