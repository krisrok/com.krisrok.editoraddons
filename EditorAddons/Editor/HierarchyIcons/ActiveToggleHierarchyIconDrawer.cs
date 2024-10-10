using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.Linq;
using System;
using static EditorAddons.Editor.HierarchyIcons;

namespace EditorAddons.Editor
{
    internal class ActiveToggleHierarchyIconDrawer : IDrawer
    {
        public DrawerAlignment Alignment => DrawerAlignment.Right;
        public int Priority => 100;
        public float Size => DefaultIconWidth;

        public void Draw(Rect rect, GameObject go)
        {
            var wasActive = go.activeSelf;
            var isActive = GUI.Toggle(rect, wasActive, "");
            if (wasActive != isActive)
            {
                // there is a selection going on -- is this object part of it?
                if (Selection.objects.Contains(go))
                {
                    for (int i = 0; i < Selection.objects.Length; i++)
                    {
                        var currentGO = Selection.objects[i] as GameObject;
                        SetActiveAndDirty(currentGO, Event.current.control ? (currentGO == go ? isActive : !isActive) : isActive);
                    }
                }
                // if this object is not part of the selection, just toggle its active state
                else
                {
                    SetActiveAndDirty(go, isActive);
                }

                if (EditorApplication.isPlaying == false)
                    EditorSceneManager.MarkSceneDirty(go.scene);
            }
        }

        private static void SetActiveAndDirty(GameObject go, bool isActive)
        {
            if (go.activeSelf == isActive)
                return;

            go.SetActive(isActive);
            if (EditorApplication.isPlaying == false)
                EditorUtility.SetDirty(go);
        }
    }
}
