using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace EditorAddons.Editor
{
    static class PrefabVariantsSearchProvider
    {
        private const string _menuItemName = "Assets/Prefabs/See variant hierarchy";
        private const string _filterPrefix = "variants";
        private const string _filterId = _filterPrefix + ":";

        [MenuItem(_menuItemName)]
        private static void OpenSearchForAsset(MenuCommand menuCommand)
        {
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);

                SearchService.ShowWindow().SetSearchText(_filterId + assetPath);
            }
        }

        [MenuItem(_menuItemName, validate = true)]
        private static bool CanOpenSearchForAsset(MenuCommand menuCommand)
        {
            if (Selection.activeObject == null)
                return false;

            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(assetPath))
                return false;

            var type = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
            return type == typeof(GameObject);
        }


        private static SearchProvider _projectProvider = null;
        private static SearchProvider _findProvider = null;
        private static int _score;
        private static SearchContext _innerContext;

        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider(_filterPrefix, "Prefab variant search")
            {
                filterId = _filterId,
                priority = 99999, // put example provider at a low priority
                showDetailsOptions = ShowDetailsOptions.Description
#if UNITY_2022_2_OR_NEWER
                    | ShowDetailsOptions.Preview
#endif
                    ,
                onDisable = OnDisable,
                trackSelection = (item, context) => PingItem(item),
                toObject = ToObject,
#if UNITY_2022_2_OR_NEWER
                fetchPreview = (item, context, size, options) =>
                {
                    switch(item.data)
                    {
                        case Component comp:
                            return SearchUtils.GetSceneObjectPreview(comp.gameObject, size, options, item.thumbnail);

                        case GameObject gameObject:
                            return SearchUtils.GetSceneObjectPreview(gameObject, size, options, item.thumbnail);
                    }

                    return null;
                },
#endif
                fetchItems = (context, items, provider) => FetchItems(context, provider)
            };
        }

        private static UnityEngine.Object PingItem(SearchItem item)
        {
            switch (item.data)
            {
                case string assetPath:
                    var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    EditorGUIUtility.PingObject(obj);
                    return obj;
            }

            return null;
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            if (context.filterId != _filterId)
                yield break;

            if (_projectProvider == null)
                _projectProvider = SearchService.GetProvider("asset");

            if (string.IsNullOrEmpty(context.searchQuery) || _projectProvider == null)
                yield break;

            var searchString = string.Join(" ", context.searchWords).Trim().ToLowerInvariant();
            _score = 1;

            // find prefab asset, match name or path
            GameObject selectedPrefab = null;
            SearchItem selectedSearchItem = null;
            GameObject basePrefab = null;
            using (_innerContext = SearchService.CreateContext(_projectProvider, $"*.prefab"))
            using (var results = SearchService.Request(_innerContext, SearchFlags.WantsMore | SearchFlags.HidePanels))
            {
                foreach (var r in results)
                {
                    if (r == null)
                    {
                        yield return null;
                        continue;
                    }

                    var gameObject = r.ToObject<GameObject>();
                    if(gameObject == null)
                    {
                        yield return null;
                        continue;
                    }

                    //Debug.Log(gameObject.name);

                    if (gameObject.name.Equals(searchString, StringComparison.OrdinalIgnoreCase) ||
                        SearchUtils.GetAssetPath(r).Equals(searchString, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedPrefab = gameObject;

                        // get base prefab
                        basePrefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(selectedPrefab);

                        if (basePrefab == selectedPrefab)
                        {
                            // create a search item
                            selectedSearchItem = provider.CreateItem(
                                            _innerContext,
                                            r.id,
                                            0,
                                            r.GetLabel(_innerContext),
                                            r.GetDescription(_innerContext),
                                            r.GetThumbnail(_innerContext),
                                            AssetDatabase.GetAssetPath(selectedPrefab));

                            MarkSelected(selectedSearchItem);

                            yield return selectedSearchItem;
                        }
                        break;
                    }

                    yield return null;
                }
            }

            if (selectedPrefab == null)
                yield break;

            // if selected prefab was not the base prefab, get the base prefab's search item (to easily get label, thumbnail etc)
            if (basePrefab != selectedPrefab)
            {
                if (_findProvider == null)
                    _findProvider = SearchService.GetProvider("find");

                using (var innerContext = SearchService.CreateContext(_findProvider, $"find:" + AssetDatabase.GetAssetPath(basePrefab)))
                using (var results = SearchService.Request(innerContext, SearchFlags.WantsMore | SearchFlags.HidePanels))
                {
                    foreach (var r in results)
                    {
                        if (r == null)
                        {
                            yield return null;
                            continue;
                        }

                        selectedSearchItem = r;
                        // add base prefab as first item
                        yield return provider.CreateItem(
                                        innerContext,
                                        r.id,
                                        0,
                                        r.GetLabel(innerContext),
                                        r.GetDescription(innerContext),
                                        r.GetThumbnail(innerContext),
                                        AssetDatabase.GetAssetPath(basePrefab));
                        break;
                    }
                }
            }
            
            var maxDepth = GetMaxDepth(context);

            // recursively look for children
            foreach (var childItem in FindChildItems(basePrefab, selectedPrefab, context, provider, maxDepth))
                yield return childItem;
        }

        static int GetMaxDepth(SearchContext context)
        {
            var match = Regex.Match(context.searchQuery, @"maxdepth:(\d+)", RegexOptions.IgnoreCase);
            if(match.Success)
            {
                if (int.TryParse(match.Groups[1].Value, out var result))
                    return result;
            }

            return -1;
        }

        private static IEnumerable<SearchItem> FindChildItems(GameObject basePrefab, GameObject selectedPrefab, SearchContext context, SearchProvider provider, int maxDepth = -1, int depth = 1)
        {
            string indent = string.Concat(Enumerable.Range(0, depth).Select(_ => "    "));
            //using (var innerContext = SearchService.CreateContext(_projectProvider, $"*.prefab"))
            using (var results = SearchService.Request(_innerContext, SearchFlags.WantsMore | SearchFlags.HidePanels))
            {
                foreach (var r in results)
                {
                    if (r == null)
                    {
                        yield return null;
                        continue;
                    }
                    var obj = r.ToObject<GameObject>();
                    if(obj == null)
                    {
                        yield return null;
                        continue;
                    }
                    var parent = PrefabUtility.GetCorrespondingObjectFromSource(obj);
                    if (parent == basePrefab)
                    {
                        var searchItem = provider.CreateItem(
                            context,
                            r.id,
                            _score++,
                            indent + "↳ " + r.GetLabel(_innerContext),
                            indent + r.GetDescription(_innerContext, true),
                            r.GetThumbnail(_innerContext),
                            AssetDatabase.GetAssetPath(obj));

                        if (obj == selectedPrefab)
                            MarkSelected(searchItem);

                        yield return searchItem;

                        if (maxDepth == -1 || depth < maxDepth)
                        {
                            foreach (var childItem in FindChildItems(obj, selectedPrefab, context, provider, maxDepth, depth + 1))
                                yield return childItem;
                        }
                    }
                }
            }
        }

        private static void MarkSelected(SearchItem searchItem)
        {
            searchItem.label = $"<b>{searchItem.label}</b>";
        }

        private static void OnDisable()
        {
            _projectProvider = null;
            _findProvider = null;
        }

        private static UnityEngine.Object ToObject(SearchItem item, Type type)
        {
            switch (item.data)
            {
                case string assetPath:
                    return AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            }

            return null;
        }
    }
}