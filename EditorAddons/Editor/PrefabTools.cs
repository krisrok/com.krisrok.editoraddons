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

            var transform = prefabStage.prefabContentsRoot.transform;

            RevertPropertyModifications(transform);
        }

        [MenuItem("Tools/Prefabs/Revert Prefab Root Transform", isValidateFunction: true)]
        private static bool CanRevertPrefabRootTransform()
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

        /// <summary>
        /// Reverts all property modifications (aka overrides) found on this GameObject.
        /// Does not include child GameObjects and their Components.
        /// </summary>
        /// <param name="gameObject"></param>
        public static void RevertAllPropertyModifications(GameObject gameObject)
        {
            var mods = PrefabUtility.GetPropertyModifications(gameObject);

            foreach (var component in gameObject.GetComponents<Component>())
            {
                RevertPropertyModifications(component, mods);
            }
        }

        /// <summary>
        /// Reverts all property modifications (aka overrides) on a specific 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="component"></param>
        public static void RevertPropertyModifications(Component component)
        {
            var mods = PrefabUtility.GetPropertyModifications(component.gameObject);

            RevertPropertyModifications(component, mods);
        }

        private static void RevertPropertyModifications(Component component, PropertyModification[] mods)
        {
            var prefabComp = PrefabUtility.GetCorrespondingObjectFromSource(component);
            using var so = new SerializedObject(component);

            foreach (var compMod in mods.Where(pm => pm.target == prefabComp))
            {
                var prop = so.FindProperty(compMod.propertyPath);
                PrefabUtility.RevertPropertyOverride(prop, InteractionMode.AutomatedAction);
            }

            so.UpdateIfRequiredOrScript();
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}