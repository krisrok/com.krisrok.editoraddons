using System;
using System.Collections.Generic;
using System.Threading;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using OdinAddons.StateUpdaters;

[assembly: RegisterStateUpdater(typeof(InspectorCounterStateUpdater), 10000)]
#endif

namespace OdinAddons
{
    /// <summary>
    /// <para>Use the <see cref="InspectorCounterAttribute"/> in conjunction with <see cref="RepaintHelper.RepaintIfVisible(object)"/> to repaint inspectors immediately on change but only when there is at least one inspector window editing your object.</para>
    /// <para>Note: You still need to manually call <see cref="RepaintHelper.RepaintIfVisible(object)"/> (see example below).</para>
    /// <example>
    /// <para>The following example demonstrates how repainting works.</para>
    /// <para>Using this setup you get immediate feedback in the inspector when NonSerializedProperty's value changes. There will be no repaint when there is no inspector window open.</para>
    /// <code>
    /// [InspectorCounter]
    /// public class MyComponent : MonoBehaviour
    /// {
    ///     private bool _nonSerializedBackingField;
    ///     
    ///     [ShowInInspector, ReadOnly]
    ///     public bool NonSerializedProperty
    ///     {
    ///         get => _nonSerializedBackingField;
    ///         set
    ///         {
    ///             _nonSerializedBackingField = value;
    ///             RepaintHelper.RepaintIfVisible(this);
    ///         }
    ///     }
    ///	}
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [IncludeMyAttributes, HideInTables]
    public class InspectorCounterAttribute : ShowInInspectorAttribute
    {
        public InspectorCounterAttribute()
        { }
    }

    public static class RepaintHelper
    {
        /// <summary>
        /// <para>Repaints the editor only if there currently is any inspector editing the <paramref name="obj"/>.</para>
        /// <para>Only works if <paramref name="obj"/> is decorated with <see cref="InspectorCounterAttribute"/>.</para>
        /// </summary>
        /// <param name="obj"></param>
        public static void RepaintIfVisible(object obj)
        {
#if UNITY_EDITOR
            if (InspectorCounterStateUpdater.HasInspector(obj) == false)
                return;

            if (SynchronizationContext.Current != InspectorCounterStateUpdater.MainSyncContext)
            {
                InspectorCounterStateUpdater.MainSyncContext.Post(_ => RepaintAllViews(), null);
            }
            else
            {
                RepaintAllViews();
            }
#endif
        }

#if UNITY_EDITOR
        private static void RepaintAllViews()
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
#endif
    }
}

#if UNITY_EDITOR
namespace OdinAddons.StateUpdaters
{
#pragma warning disable
    public sealed class InspectorCounterStateUpdater : AttributeStateUpdater<InspectorCounterAttribute>, IDisposable
    {
        private static Dictionary<int, int> _inspectorCounterMap = new Dictionary<int, int>();
        private static SynchronizationContext _mainSyncContext;

        internal static SynchronizationContext MainSyncContext => _mainSyncContext;

        internal static bool HasInspector(object obj)
        {
            if (_inspectorCounterMap.TryGetValue(obj.GetHashCode(), out var counter) == false || counter == 0)
                return false;

            return true;
        }

        protected override void Initialize()
        {
            if (_mainSyncContext == null)
            {
                _mainSyncContext = SynchronizationContext.Current;
            }

            UpdateCounter(+1);
        }

        public void Dispose()
        {
            UpdateCounter(-1);
        }

        private void UpdateCounter(int delta)
        {
            var key = this.Property.SerializationRoot.ValueEntry.WeakSmartValue.GetHashCode();
            _inspectorCounterMap.TryGetValue(key, out int counter);
            counter += delta;
            _inspectorCounterMap[key] = counter;
            //UnityEngine.Debug.Log(key + " => " + counter);
        }
    }
}
#endif