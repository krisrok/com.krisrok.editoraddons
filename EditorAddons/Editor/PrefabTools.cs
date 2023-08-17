using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace EditorAddons.Editor
{
    public static class PrefabTools
    {
        [MenuItem("Tools/Prefabs/Revert Prefab Root Transform")]
        public static void RevertPrefabRootTransform()
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage == null)
                return;

            //var overrides = PrefabUtility.GetObjectOverrides(prefabStage.prefabContentsRoot, true);
            //var to = overrides.Where(o => o.instanceObject == prefabStage.prefabContentsRoot.transform || o.instanceObject == prefabStage.prefabContentsRoot.GetComponent<RectTransform>()).ToList();
            //PrefabUtility.RevertObjectOverride(prefabStage.prefabContentsRoot.transform, InteractionMode.UserAction);
            //PrefabUtility.RevertObjectOverride(prefabStage.prefabContentsRoot.GetComponent<RectTransform>(), InteractionMode.UserAction);

            //var so = new SerializedObject(prefabStage.prefabContentsRoot.GetComponent<RectTransform>());
            //var prop = so.FindProperty("m_sizeDelta");
            //PrefabUtility.RevertPropertyOverride(prop, InteractionMode.UserAction);

            var transform = prefabStage.prefabContentsRoot.transform;
            var rectTransform = transform.GetComponent<RectTransform>();

            var originalTransform = PrefabUtility.GetCorrespondingObjectFromSource(transform);

            var mods = PrefabUtility.GetPropertyModifications(prefabStage.prefabContentsRoot.transform);
            var transformMods = mods
                .Where(pm => pm.target == originalTransform)
                .ToList();

            transformMods.ForEach(p =>
            {
                var so = new SerializedObject(transform);
                var prop = so.FindProperty(p.propertyPath);
                PrefabUtility.RevertPropertyOverride(prop, InteractionMode.UserAction);
                so.Dispose();
            });
        }

        [MenuItem("Tools/Prefabs/Revert Prefab Root Transform", isValidateFunction: true)]
        public static bool CanDoStuff()
        {
            var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            return prefabStage != null;
        }

        [MenuItem("Tools/Prefabs/Reimport all Prefabs")]
        private static void ReimportAllPrefabs()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                var prefabPaths = new List<string>();
                foreach (string _prefabPath in GetAllPrefabPaths())
                {
                    GameObject _prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                    var type = PrefabUtility.GetPrefabAssetType(_prefabAsset);
                    if (type == PrefabAssetType.Regular || type == PrefabAssetType.Variant)
                    {
                        prefabPaths.Add(_prefabPath);
                    }
                }

                foreach (string _prefabPath in prefabPaths)
                    AssetDatabase.ImportAsset(_prefabPath, ImportAssetOptions.ForceUpdate);
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        [MenuItem("Tools/Prefabs/Reserialize all Prefabs")]
        private static void ReserializeAllPrefabs()
        {
            try
            {
                AssetDatabase.StartAssetEditing();

                foreach (string _prefabPath in GetAllPrefabPaths())
                {
                    GameObject _prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(_prefabPath);
                    if (!PrefabUtility.IsPartOfImmutablePrefab(_prefabAsset))
                    {
                        PrefabUtility.SavePrefabAsset(_prefabAsset);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
        }

        public static string[] GetAllPrefabPaths()
        {
            List<string> _prefabPaths = new List<string>();
            foreach (string _paths in AssetDatabase.GetAllAssetPaths())
            {
                if (_paths.EndsWith(".prefab"))
                {
                    _prefabPaths.Add(_paths);
                }
            }
            return _prefabPaths.ToArray();
        }
    }
}