#if ODIN_INSPECTOR_3_2
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace OdinAddons.Editor
{
    public class FindHiddenObjectsWindow : OdinEditorWindow
    {

        [MenuItem("Tools/Find hidden GameObjects")]
        private static void Open()
        {
            var window = EditorWindow.GetWindow<FindHiddenObjectsWindow>();
            window.Show();
        }

        [Serializable]
        internal class HiddenObject
        {
            public GameObject GameObject;

            [OnValueChanged(nameof(ValueChanged))]
            public HideFlags HideFlags;

            private void ValueChanged()
            {
                GameObject.hideFlags = HideFlags;
                EditorUtility.SetDirty(GameObject);
            }
        }

        [SerializeField]
        private List<HiddenObject> _hiddenObjects;

        protected override void Initialize()
        {
            base.Initialize();
            UpdateList();
        }

        [Button]
        private void UpdateList()
        {
            _hiddenObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects()
                .Where(ro => ro.hideFlags != HideFlags.None)
                .Select(ro => new HiddenObject { GameObject = ro, HideFlags = ro.hideFlags })
                .ToList();
        }

        protected override void OnImGUI()
        {
            EditorGUILayout.HelpBox(new GUIContent("Searches for hidden root level GameObjects"));

            base.OnImGUI();
        }
    }
}
#endif