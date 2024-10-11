using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

namespace EditorAddons.Editor
{
    [InitializeOnLoad]
    public partial class HierarchyIcons
    {
        public enum DrawerAlignment { Left, Right, AfterLabel }

        public interface IDrawer
        {
            DrawerAlignment Alignment { get; }
            int Priority { get; }
            float MinWidth { get; }
            Rect Draw(Rect rect, GameObject go);
        }

        public const float DefaultIconWidth = 15;

        private static readonly List<IDrawer> _drawers = new List<IDrawer>();
        private static readonly Dictionary<DrawerAlignment, List<IDrawer>> _drawerAlignmentMap = new Dictionary<DrawerAlignment, List<IDrawer>>();
        
        private static ObjectIconHierarchyIconDrawer _objectIconDrawer;
        private static ActiveToggleHierarchyIconDrawer _activeToggleDrawer;
        private static bool _hasRegisteredListener;

        static HierarchyIcons()
        {
            HierarchyIconsSettings.Instance.Changed += ApplySettings;
            ApplySettings();
        }

        private static void ApplySettings()
        {
            var settings = HierarchyIconsSettings.Instance;

            if (settings.DrawObjectIcon)
            {
                _objectIconDrawer ??= new ObjectIconHierarchyIconDrawer();
                RegisterDrawer(_objectIconDrawer);
            }
            else if (_objectIconDrawer != null)
            {
                UnregisterDrawer(_objectIconDrawer);
                _objectIconDrawer = null;
            }

            if (settings.DrawActiveToggle)
            {
                _activeToggleDrawer ??= new ActiveToggleHierarchyIconDrawer();
                RegisterDrawer(_activeToggleDrawer);
            }
            else if (_activeToggleDrawer != null)
            {
                UnregisterDrawer(_activeToggleDrawer);
                _activeToggleDrawer = null;
            }
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
            DrawAfterLabelDrawers(rect, go);
        }

        private static void DrawRightDrawers(Rect rect, GameObject go)
        {
            if (_drawerAlignmentMap.TryGetValue(DrawerAlignment.Right, out var drawers) == false || _drawerAlignmentMap.Count == 0)
                return;

            foreach (var drawer in drawers)
            {
                var drawerRect = rect;
                drawerRect.x = drawerRect.width - drawer.MinWidth + drawerRect.x;
                drawerRect.width = drawer.MinWidth;

                var usedRect = drawer.Draw(drawerRect, go);
                rect.xMax = usedRect.xMin;
            }
        }

        private static void DrawLeftDrawers(Rect rect, GameObject go)
        {
            if (_drawerAlignmentMap.TryGetValue(DrawerAlignment.Left, out var drawers) == false || _drawerAlignmentMap.Count == 0)
                return;

            rect.x = 0;

            foreach (var drawer in drawers)
            {
                var drawerRect = rect;
                drawerRect.width = drawer.MinWidth;

                var usedRect = drawer.Draw(drawerRect, go);
                rect.xMin = usedRect.xMax;
            }
        }

        private static void DrawAfterLabelDrawers(Rect rect, GameObject go)
        {
            if (_drawerAlignmentMap.TryGetValue(DrawerAlignment.AfterLabel, out var drawers) == false || _drawerAlignmentMap.Count == 0)
                return;

            rect.xMin += 16 + EditorStyles.largeLabel.CalcSize(new GUIContent(go.name)).x;

            foreach (var drawer in drawers)
            {
                var drawerRect = rect;
                drawerRect.width = drawer.MinWidth;

                var usedRect = drawer.Draw(drawerRect, go);
                rect.xMin = usedRect.xMax;
            }
        }

        public static void RegisterDrawer(IDrawer drawer)
        {
            if (_drawers.Contains(drawer))
                return;

            _drawers.Add(drawer);

            if (_drawerAlignmentMap.TryGetValue(drawer.Alignment, out var list) == false)
                list = _drawerAlignmentMap[drawer.Alignment] = new List<IDrawer>();

            list.Add(drawer);
            list.Sort(CompareDrawerPriority);

            int CompareDrawerPriority(IDrawer x, IDrawer y)
            {
                return y.Priority.CompareTo(x.Priority);
            }

            UpdateEventListenerRegistration();
        }

        public static void UnregisterDrawer(IDrawer drawer)
        {
            if (_drawers.Remove(drawer) == false)
                return;

            if (_drawerAlignmentMap.TryGetValue(drawer.Alignment, out var list) == false)
                return;

            list.Remove(drawer);

            UpdateEventListenerRegistration();
        }

        private static void UpdateEventListenerRegistration()
        {
            if (_drawers.Count > 0 && _hasRegisteredListener == false)
            {
                EditorApplication.hierarchyWindowItemOnGUI += EditorApplication_hierarchyWindowItemOnGUI;
                _hasRegisteredListener = true;
                return;
            }

            if (_drawers.Count == 0 && _hasRegisteredListener == true)
            {
                EditorApplication.hierarchyWindowItemOnGUI -= EditorApplication_hierarchyWindowItemOnGUI;
                _hasRegisteredListener = false;
            }
        }

        private static void RegisterDebugDrawers()
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
            RegisterDrawer(new DebugDrawer
            {
                Alignment = DrawerAlignment.Left,
                Color = Color.blue
            });
        }
        
        private class DebugDrawer : IDrawer
        {
            public DrawerAlignment Alignment { get; set; }
            public int Priority { get; set; }
            public float MinWidth { get; set; } = DefaultIconWidth;
            public Color Color { get; set; } = Color.magenta;

            public Rect Draw(Rect rect, GameObject go)
            {
                var tmpColor = GUI.color;
                GUI.color = Color;
                GUI.Toggle(rect, false, GUIContent.none);
                GUI.color = tmpColor;

                return rect;
            }
        }

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
