using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using Sirenix.OdinInspector.Editor;
#endif

namespace OdinAddons
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class ResettableAttribute : Attribute
    { }

#if UNITY_EDITOR
    public static class InspectorPropertyResetExtensions
    {
        public static void ResetToDefaultValue(this InspectorProperty property)
        {
            var tree = property.Tree;
            CreateInstance(tree.TargetType, cloneObj =>
            {
                ApplyModifiedProperties(cloneObj);
                using var cloneTree = PropertyTree.Create(cloneObj);
                var cloneProperty = cloneTree.GetPropertyAtPath(property.Path);
                try
                {
                    if (cloneProperty == null)
                    {
                        string pathSeparator = ".";
                        var currentProperty = property;
                        var parentChain = new List<Type>();
                        var pathParts = property.Path.Split(pathSeparator);
                        int i = pathParts.Length - 1;
                        while (i > 0 && currentProperty.IsTreeRoot == false && cloneProperty == null)
                        {
                            var parentType = currentProperty.ParentType;
                            currentProperty = currentProperty.Parent;

                            var parentCloneObj = Activator.CreateInstance(parentType);
                            var parentCloneTree = PropertyTree.Create(parentCloneObj);

                            var path = string.Join(pathSeparator, pathParts.Skip(i));
                            cloneProperty = parentCloneTree.GetPropertyAtPath(path);
                            i--;
                        }
                    }

                    if (cloneProperty == null)
                    {
                        Debug.LogError("Unable to find default value for " + property.Path);
                        return;
                    }

                    property.ValueEntry.WeakSmartValue = cloneProperty.ValueEntry.WeakSmartValue;
                    property.MarkSerializationRootDirty();
                }
                finally
                {
                    cloneProperty?.Dispose();
                }
            });
        }

        private static void CreateInstance(Type targetType, Action<UnityEngine.Object> action)
        {
            if (typeof(ScriptableObject).IsAssignableFrom(targetType))
            {
                var cloneObj = ScriptableObject.CreateInstance(targetType);

                try
                {
                    action(cloneObj);
                }
                finally
                {
                    SafeDestroy(cloneObj);
                }
                return;
            }
            else if (typeof(Component).IsAssignableFrom(targetType))
            {
                var ps = ScriptableObject.CreateInstance<PreviewStage>();
                StageUtility.GoToStage(ps, false);
                var cloneObj = ps.CreateComponent(targetType);

                try
                {
                    action(cloneObj);
                }
                finally
                {
                    StageUtility.GoBackToPreviousStage();
                    SafeDestroy(ps);
                }
                return;
            }

            throw new NotSupportedException("Type not supported: " + targetType.FullName);
        }

        private static void ApplyModifiedProperties(UnityEngine.Object obj)
        {
            using var so = new SerializedObject(obj);
            so.ApplyModifiedProperties();
        }

        private static void SafeDestroy(UnityEngine.Object obj)
        {
            if (Application.isPlaying == false)
                UnityEngine.Object.DestroyImmediate(obj);
            else
                UnityEngine.Object.Destroy(obj);
        }

        private class PreviewStage : PreviewSceneStage
        {
            protected override GUIContent CreateHeaderContent()
            {
                return new GUIContent();
            }

            public Component CreateComponent(Type componentType)
            {
                StageUtility.PlaceGameObjectInCurrentStage(new GameObject());
                var gameObject = scene.GetRootGameObjects()[0];
                return gameObject.AddComponent(componentType);
            }
        }
    }
#endif
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public class ResettableAttributeDrawer : OdinAttributeDrawer<ResettableAttribute>, IDefinesGenericMenuItems
    {
        private static readonly GUIContent _resetGuiContent = new GUIContent("Reset");

        protected override void Initialize()
        {
            SkipWhenDrawing = true;
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddSeparator(string.Empty);
            genericMenu.AddItem(_resetGuiContent, false, ResetToDefaultValue);
        }

        private void ResetToDefaultValue()
        {
            Property.ResetToDefaultValue();
        }
    }
}
#endif