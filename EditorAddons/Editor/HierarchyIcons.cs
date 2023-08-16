using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using System.Linq;
using System;

namespace EditorAddons.Editor
{
    [InitializeOnLoad]
    class HierarchyIcons
    {
        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += EditorApplication_hierarchyWindowItemOnGUI;
        }

        static Dictionary<int, int> _hoverStartTimeMap = new Dictionary<int, int>();

        static void EditorApplication_hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //UpdateMouseHover(instanceID, selectionRect);

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
                return;

            DrawActiveToggle(selectionRect, go);
            //DrawObjectIcon(selectionRect, go);
        }

        private static string defaultIconName = EditorGUIUtility.isProSkin ? "d_GameObject Icon (UnityEngine.Texture2D)" : "GameObject Icon (UnityEngine.Texture2D)";

        private static void DrawObjectIcon(Rect selectionRect, GameObject go)
        {
            var mr = new Rect(selectionRect);
            mr.x -= 200;
            mr.width += 400;
            if (mr.Contains(Event.current.mousePosition))
                return;

            var img = EditorGUIUtility.ObjectContent(go, go.GetType()).image;
            if (img.ToString() != defaultIconName)
            {
                var r = new Rect(selectionRect);
                r.x = 0;
                r.width = 20;
                GUI.Label(r, img);
            }
        }

        private static void DrawActiveToggle(Rect selectionRect, GameObject go)
        {
            Rect r = new Rect(selectionRect);
            r.x = r.width - 20 + r.x;
            r.width = 20;

            var wasActive = go.activeSelf;
            var isActive = GUI.Toggle(r, wasActive, "");
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

        private static void UpdateMouseHover(int instanceID, Rect selectionRect)
        {
            if (selectionRect.Contains(Event.current.mousePosition))
            {
                if (Event.current.isMouse && Event.current.button == 0 && Event.current.shift)
                {
                    if (ActiveEditorTracker.sharedTracker.isLocked == false || ActiveEditorTracker.sharedTracker.activeEditors[0].target != EditorUtility.InstanceIDToObject(instanceID))
                    {
                        ActiveEditorTracker.sharedTracker.isLocked = false;
                        Selection.activeGameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
                        ActiveEditorTracker.sharedTracker.isLocked = true;
                    }
                    else

                    {
                        if (ActiveEditorTracker.sharedTracker.isLocked)
                        {
                            ActiveEditorTracker.sharedTracker.isLocked = false;
                        }
                    }
                }

                var time = EditorApplication.timeSinceStartup;
                if (_hoverStartTimeMap.ContainsKey(instanceID))
                {
                    if (time - _hoverStartTimeMap[instanceID] > 2)
                    {
                        if (Selection.activeGameObject != EditorUtility.InstanceIDToObject(instanceID))
                        {
                            Selection.activeGameObject = (GameObject)EditorUtility.InstanceIDToObject(instanceID);
                            ActiveEditorTracker.sharedTracker.isLocked = true;
                        }
                    }
                }
                else
                {
                    _hoverStartTimeMap[instanceID] = (int)time;
                }

            }
            else
            {
                if (_hoverStartTimeMap.ContainsKey(instanceID))
                    _hoverStartTimeMap.Remove(instanceID);
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
