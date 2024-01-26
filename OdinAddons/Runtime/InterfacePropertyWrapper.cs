using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor.ValueResolvers;
#endif

namespace OdinAddons
{
    /// <summary>
    /// Odin-dependent helper to handle interface references in the inspector.
    /// It supports unity-serialized objects based on <see cref="UnityEngine.Object"/> as well as plain classes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    [InlineProperty(LabelWidth = 85)]
    public class InterfacePropertyWrapper<T>
        where T : class
    {
        private Type _type => typeof(T);

        private Type[] _disallowedTypes;

        [ValidateType(nameof(_type), AutoSelectMatchingComponent = true)]
        [HideLabel]
        [ShowIf(nameof(_valueUnityObject))]
        [SerializeField]
        private UnityEngine.Object _valueUnityObject;

        [SerializeReference, HideInInspector]
        private T _valueNonUnityObject;

        [HideLabel]
        [ShowInInspector]
        [HideIf(nameof(_valueUnityObject))]
#if UNITY_EDITOR
        [ValidateInput(nameof(Editor_ValidateParentRequiredAttribute), DefaultMessage = "@" + nameof(Editor_RequiredMessage))]
        [ValidateInput(nameof(Editor_ValidateValueTypeIsAllowed), DefaultMessage = "@" + nameof(Editor_DisallowedTypeMessage))]
#endif
        public T Value
        {
            get
            {
                if (_valueUnityObject != null)
                    return _valueUnityObject as T;

                return _valueNonUnityObject;
            }
            set
            {
                if (value == null)
                {
                    // Do we have a non-null value somewhere? Then null the references and raise changed event.
                    if ((object)_valueUnityObject != null || _valueNonUnityObject != null)
                    {
                        _valueUnityObject = null;
                        _valueNonUnityObject = null;

                        ValueChanged?.Invoke(this, null);
                    }

                    // Everything was nulled beforehand or is nulled now. Nothing left to do.
                    return;
                }

                // Do nothing if the value is already referenced.
                if (value == (object)_valueUnityObject || value == _valueNonUnityObject)
                {
                    return;
                }
                // Special treatment if value is a unity object.
                else if (value is UnityEngine.Object)
                {
                    _valueUnityObject = value as UnityEngine.Object;
                    _valueNonUnityObject = null;

                    // Try to find a matching component on a given GameObject.
                    if (_valueUnityObject is GameObject)
                    {
                        var foundComponent = (_valueUnityObject as GameObject).GetComponent<T>();
                        if (foundComponent != null)
                        {
                            // The component we found is already set, so do nothing.
                            if (foundComponent == (object)_valueUnityObject)
                                return;

                            _valueUnityObject = foundComponent as Component;
                        }
                    }
                }
                // Otherwise just set the non-unity object and null the unity reference.
                else
                {
                    _valueUnityObject = null;
                    _valueNonUnityObject = value;
                }

                ValueChanged?.Invoke(this, value);
            }
        }

        public delegate void ValueChangedEventHandler(InterfacePropertyWrapper<T> source, T value);

        public event ValueChangedEventHandler ValueChanged;

        public InterfacePropertyWrapper()
        { }

        public InterfacePropertyWrapper(Type[] disallowedTypes)
        {
            _disallowedTypes = disallowedTypes;
        }

        public InterfacePropertyWrapper(T value, Type[] typeFilter = null)
            : this(disallowedTypes: typeFilter)
        {
            Value = value;
        }

#if UNITY_EDITOR
        private bool Editor_ValidateParentRequiredAttribute(Sirenix.OdinInspector.Editor.InspectorProperty property)
        {
            var requiredAttribute = property.Parent.GetAttribute<RequiredAttribute>();
            if (requiredAttribute == null)
                return true;

            Editor_RequiredMessage = requiredAttribute.ErrorMessage ?? $"{property.Parent.NiceName} is required";

            return _valueUnityObject != null || _valueNonUnityObject != null;
        }

        private bool Editor_ValidateValueTypeIsAllowed(object value)
        {
            if (value == null || _disallowedTypes == null)
                return true;

            if (_disallowedTypes.Any(t => t.IsAssignableFrom(value.GetType())) == false)
                return true;

            Editor_DisallowedTypeMessage = $"{value.GetType()} is not allowed";
            return false;
        }

        [OnInspectorInit]
        private void OnInspectorInit(Sirenix.OdinInspector.Editor.InspectorProperty property)
        {
            Editor_TypeFilter = getEditor_TypeFilter(property);
        }

        private IEnumerable<Type> Editor_TypeFilter;
        private IEnumerable<Type> getEditor_TypeFilter(Sirenix.OdinInspector.Editor.InspectorProperty property)
        {
            var typeFilterAttribute = property.Parent.GetAttribute<TypeFilterAttribute>();
            if (typeFilterAttribute == null)
                return null;

            //AssemblyUtilities.GetTypes()

            var rawGetter = ValueResolver.Get<object>(property, typeFilterAttribute.FilterGetter);
            return rawGetter.GetValue() as IEnumerable<Type>;
        }

        private string Editor_RequiredMessage;
        private string Editor_DisallowedTypeMessage;
#endif
    }
}