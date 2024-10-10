using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace EditorAddons.Editor
{
    [InitializeOnLoad]
    public partial class HierarchyIcons
    {
        public enum DrawerAlignment { Left, Right }

        public interface IDrawer
        {
            DrawerAlignment Alignment { get; }
            int Priority { get; }
            float Size { get; }
            void Draw(Rect rect, GameObject go);
        }

        public const float DefaultIconWidth = 15;

        private static readonly Dictionary<DrawerAlignment, List<IDrawer>> _drawers = new Dictionary<DrawerAlignment, List<IDrawer>>();


        static HierarchyIcons()
        {
            EditorApplication.hierarchyWindowItemOnGUI += EditorApplication_hierarchyWindowItemOnGUI;

            RegisterDrawer(new ActiveToggleHierarchyIconDrawer());
            RegisterDrawer(new ObjectIconHierarchyIconDrawer());
        }

        static void EditorApplication_hierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            //UpdateMouseHover(instanceID, selectionRect);

            GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (go == null)
                return;
            
            DrawDrawers(selectionRect, go);
        }

        private static void DrawDrawers(Rect rect, GameObject go)
        {
            DrawRightDrawers(rect, go);
            DrawLeftDrawers(rect, go);
        }

        private static void DrawRightDrawers(Rect rect, GameObject go)
        {
            if (_drawers.TryGetValue(DrawerAlignment.Right, out var rightDrawers) == false)
                return;

            foreach (var drawer in rightDrawers)
            {
                var drawerRect = rect;
                drawerRect.x = drawerRect.width - drawer.Size + drawerRect.x;
                drawerRect.width = drawer.Size;
                rect.xMax = drawerRect.xMin;

                drawer.Draw(drawerRect, go);
            }
        }

        private static void DrawLeftDrawers(Rect rect, GameObject go)
        {
            if (_drawers.TryGetValue(DrawerAlignment.Left, out var leftDrawers) == false)
                return;

            rect.x -= 59;

            foreach (var drawer in leftDrawers)
            {
                var drawerRect = rect;
                drawerRect.width = drawer.Size;
                rect.xMin = drawerRect.xMax;

                drawer.Draw(drawerRect, go);
            }
        }

        public static void RegisterDrawer(IDrawer drawer)
        {
            if (_drawers.TryGetValue(drawer.Alignment, out var list) == false)
                list = _drawers[drawer.Alignment] = new List<IDrawer>();

            list.Add(drawer);
            list.Sort(CompareDrawerPriority);

            int CompareDrawerPriority(IDrawer x, IDrawer y)
            {
                return y.Priority.CompareTo(x.Priority);
            }
        }

        public static void UnregisterDrawer(IDrawer drawer)
        {
            if (_drawers.TryGetValue(drawer.Alignment, out var list) == false)
                return;

            list.Remove(drawer);
        }

        /*
        private void RegisterDebugDrawers()
        {
            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Right,
                Color = Color.red
            });
            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Right,
                Color = Color.green
            });
            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Right,
                Color = Color.blue
            });

            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Left,
                Color = Color.red
            });
            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Left,
                Color = Color.green
            });
        }
        
        private class DebugDrawer : IDrawer
        {
            public DrawerAlignment Alignment { get; set; }
            public int Priority { get; set; }
            public float Size { get; set; } = 15;
            public Color Color { get; set; } = Color.magenta;

            public void Draw(Rect rect, GameObject go)
            {
                var tmpColor = GUI.color;
                GUI.color = Color;
                GUI.Toggle(rect, false, GUIContent.none);
                GUI.color = tmpColor;
            }
        }
        */

        /*
        static Dictionary<int, int> _hoverStartTimeMap = new Dictionary<int, int>();

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
        */
    }
}
