using System;
using System.Linq;

using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif

namespace OdinAddons
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class SetNameFromStringAttribute : Attribute
    { }
#if UNITY_EDITOR
#endif
}

#if UNITY_EDITOR
namespace OdinAddons.Drawers
{
    public class SetNameFromStringDrawer : OdinAttributeDrawer<SetFromPatternAttribute>, IDefinesGenericMenuItems
    {
        private static readonly GUIContent _guiContent = new GUIContent("Set name from string");

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
            genericMenu.AddItem(_guiContent, false, SetName);
        }

        private void SetName()
        {
            var root = Property.SerializationRoot;
            Undo.RecordObjects(root.ValueEntry.WeakValues.OfType<Component>().Select(c => c.gameObject).ToArray(), "Set name from pattern");
            for (int i = 0; i < Property.ValueEntry.ValueCount; i++)
            {
                (root.ValueEntry.WeakValues[i] as Component).gameObject.name = Property.ValueEntry.WeakValues[i].ToString();
            }
        }
    }
}
#endif