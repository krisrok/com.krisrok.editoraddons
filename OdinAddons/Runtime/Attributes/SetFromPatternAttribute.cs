using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace OdinAddons
{
    /// <summary>
    /// Use a pattern to set a string's value
    /// </summary>
    /// <example>
    /// <code>{n}</code> resolves to the GameObject's name.
    /// </example>
    /// <example>
    /// <code>{n^}</code> resolves to the parent's GameObject's name (chainable)
    /// </example>
    /// <example>
    /// <code>{n} => (\d+)$</code> uses a regex pattern to capture the GameObject's name's ending digits
    /// </example>
    /// <example>
    /// <code>{n} => ^(\d+).*?(\d+)$</code> uses a regex pattern to capture the GameObject's name's starting and ending digits and joins them
    /// </example>
    /// /// <example>
    /// <code>{n} => ^(\d+).*?(\d+)$ => {0}_{1}</code> uses a regex pattern to capture the GameObject's name's starting and ending digits and joins them with an underscore using a string format
    /// </example>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SetFromPatternAttribute : Attribute
    { }

#if UNITY_EDITOR
    public class SetFromPatternTools
    {
        public static string TransformPattern(string input, GameObject context)
        {
            var patternParts = input.Split(" => ");

            var resolvedString = ResolveFromInputFormatAndContext(patternParts[0], context);

            if (patternParts.Length == 1)
                return resolvedString;

            var captures = GetCaptures(resolvedString, patternParts[1]);

            if (patternParts.Length == 2)
                return string.Join("", captures);

            return FormatCaptures(captures, patternParts[2]);
        }

        private static string ResolveFromInputFormatAndContext(string inputFormat, GameObject context)
        {
            foreach (var p in _replacePairs)
            {
                inputFormat = p.Key.Replace(inputFormat, match => p.Value(match, context, null));
            }

            return inputFormat;
        }

        private static IEnumerable<string> GetCaptures(string input, string pattern)
        {
            var match = Regex.Match(input, pattern);
            if (match.Success == false)
                throw new ArgumentException($"Regex pattern ({pattern}) does not match input ({input})");

            return match.Groups.Skip(1).Select(g => g.Value);
        }

        private static string FormatCaptures(IEnumerable<string> inputs, string format)
        {
            return string.Format(format, inputs.ToArray());
        }

        private delegate string Replac0r(Match replaceMatch, GameObject context, Dictionary<string, object> userData);
        private static readonly RegexOptions _defaultOptions = RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Compiled;

        private static Dictionary<Regex, Replac0r> _replacePairs = new Dictionary<Regex, Replac0r>()
        {
            {
                new Regex(@"({(NAME|N)(?<Parent>\.PARENT|\^)*})", _defaultOptions),
                (replaceMatch, context, userData) =>
                {
                    var t = context.transform;

                    for(var i=0;i<replaceMatch.Groups["Parent"].Captures.Count;i++)
                        t = t.parent;

                    return t.name;
                }
            },
            {
                new Regex(@"({(INDEX|I)(?<Parent>\.PARENT|\^)*(?<Offset>[\+-]\d+)(?<Format>:.*?)?})", _defaultOptions),
                (replaceMatch, context, userData) =>
                {
                    var t = context.transform;

                    for(var i=0;i<replaceMatch.Groups["Parent"].Captures.Count;i++)
                        t = t.parent;

                    var index = t.GetSiblingIndex();

                    if(replaceMatch.Groups["Offset"].Success)
                    {
                        index += int.Parse(replaceMatch.Groups["Offset"].Value);
                    }

                    if(replaceMatch.Groups["Format"].Success)
                    {
                        return index.ToString(replaceMatch.Groups["Format"].Value.Substring(1));
                    }

                    return index.ToString();
                }
            }
        };
    }
#endif
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public class SetFromPatternAttributeDrawer : OdinAttributeDrawer<SetFromPatternAttribute>, IDefinesGenericMenuItems
    {
        private static readonly GUIContent _guiContent = new GUIContent("Set from pattern");

        protected override void Initialize()
        {
            SkipWhenDrawing = true;
        }

        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            if (property.ValueEntry.TypeOfValue != typeof(string))
                return false;

            return base.CanDrawAttributeProperty(property);
        }

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddSeparator(string.Empty);
            genericMenu.AddItem(_guiContent, false, SetFromPattern);
        }

        private void SetFromPattern()
        {
            var root = Property.SerializationRoot;
            for(int i=0;i<Property.ValueEntry.ValueCount;i++) 
            {
                Property.ValueEntry.WeakValues[i] = SetFromPatternTools.TransformPattern(Property.ValueEntry.WeakValues[i] as string, (root.ValueEntry.WeakValues[i] as Component).gameObject);
            }
        }
    }
}
#endif