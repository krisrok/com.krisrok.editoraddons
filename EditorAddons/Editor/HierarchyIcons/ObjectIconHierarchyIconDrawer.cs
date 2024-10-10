using UnityEditor;
using UnityEngine;
using static EditorAddons.Editor.HierarchyIcons;

namespace EditorAddons.Editor
{
    internal class ObjectIconHierarchyIconDrawer : IDrawer
    {
        private static string defaultIconName = EditorGUIUtility.isProSkin ? "d_GameObject Icon (UnityEngine.Texture2D)" : "GameObject Icon (UnityEngine.Texture2D)";

        public DrawerAlignment Alignment => DrawerAlignment.Left;
        public int Priority => 100;
        public float Size => DefaultIconWidth;

        public void Draw(Rect rect, GameObject go)
        {
            var mr = new Rect(rect);
            mr.x -= 200;
            mr.width += 400;
            if (mr.Contains(Event.current.mousePosition))
                return;

            var img = EditorGUIUtility.ObjectContent(go, go.GetType()).image;
            if (img.ToString() != defaultIconName)
            {
                var r = rect;
                GUI.Label(r, img);
            }
        }
    }
}
