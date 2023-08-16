using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace EditorAddons.Editor
{
    /// <summary>
    /// <para>Searches the hierarchy and assets for GUID.</para>
    /// <para>
    /// Hierarchy search: Useful when debugging. When hitting a breakpoint,
    /// copy the GameObject's or Component's GUID retrieved from 
    /// <see cref="UnityEngine.Object.GetInstanceID()">GetInstanceID()</see>.
    /// Then search for "guid:[GUID]" to find the corresponding GameObject/Component
    /// in the hierarchy.
    /// </para>
    /// <para>
    /// Asset search: Copy the GUID from a .meta and search for "guid:[GUID]".
    /// </para>
    /// </summary>
    static class GuidSearchProvider
    {
        private static SearchProvider projectProvider = null;

        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider("guid", "GUID search")
            {
                filterId = "guid:",
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
            switch(item.data)
            {
                case Component comp:
                    EditorGUIUtility.PingObject(comp);
                    return comp;

                case GameObject gameObject:
                    EditorGUIUtility.PingObject(gameObject);
                    return gameObject;

                case string assetPath:
                    var obj = AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
                    EditorGUIUtility.PingObject(obj);
                    return obj;
            }

            return null;
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            if (projectProvider == null)
                projectProvider = SearchService.GetProvider("scene");

            if (string.IsNullOrEmpty(context.searchQuery) || projectProvider == null)
                yield break;

            var guidString = context.searchQuery.Trim().ToLowerInvariant();
            //var regex = WildCardToRegular(guidString);

            List<Component> comps = new List<Component>();
            using (var innerContext = SearchService.CreateContext(projectProvider, $"h:t:{nameof(GameObject)}"))
            using (var results = SearchService.Request(innerContext))
            {
                foreach(var r in results)
                {
                    if (r == null)
                    {
                        yield return null;
                        continue;
                    }
                    var gameObject = EditorUtility.InstanceIDToObject(int.Parse(r.id)) as GameObject;
                    if (gameObject == null)
                    {
                        yield return null;
                        continue;
                    }

                    int instanceId = gameObject.GetInstanceID();
                    if (IsEqual(guidString, instanceId))
                    {
                        yield return provider.CreateItem(context, r.id, instanceId.ToString().CompareTo(guidString),
                                r.GetLabel(innerContext, true), "Game Object",
                                null, gameObject);

                        break;
                    }

                    gameObject?.GetComponents(comps);
                    foreach(var c in comps)
                    {
                        instanceId = c.GetInstanceID();
                        if (IsEqual(guidString, instanceId))
                        {
                            yield return provider.CreateItem(context, r.id, instanceId.ToString().CompareTo(guidString),
                                r.GetLabel(innerContext, true), ObjectNames.NicifyVariableName(c.GetType().Name),
                                null, c);
                        }
                        else
                        {
                            yield return null;
                        }
                    }
                }
            }

            var assetPath = AssetDatabase.GUIDToAssetPath(guidString);
            if(string.IsNullOrEmpty(assetPath) == false)
            {
                yield return provider.CreateItem(context, assetPath, 1, assetPath, assetPath, null, assetPath);
            }
        }

        private static bool IsEqual(string guidString, int instanceId)
        {
            return instanceId.ToString().ToLowerInvariant() == guidString;
        }

        private static void OnDisable()
        {
            projectProvider = null;
        }

        private static UnityEngine.Object ToObject(SearchItem item, Type type)
        {
            switch(item.data)
            {
                case Component comp:
                    return comp.gameObject;

                case GameObject gameObject:
                    return gameObject;

                case string assetPath:
                    return AssetDatabase.LoadAssetAtPath(assetPath, typeof(UnityEngine.Object));
            }

            return null;
        }
    }
}