using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EditorAddons.Editor
{
    /// <summary>
    /// Enables edits without marking the surrounding scene/prefab dirty.
    /// </summary>
    public static class DrivenPropertyManagerProxy
    {
        private static Action<Object, Object, string> _registerPropertyAction;

        private static Action<Object, Object, string> _unregisterPropertyAction;

        private static bool _isInited;

        static DrivenPropertyManagerProxy()
        {
            InitDelegates();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void InitDelegates()
        {
            if (_isInited)
                return;

            _isInited = true;

            var dpmType = typeof(Object).Assembly.GetType("UnityEngine.DrivenPropertyManager");
            CreateDelegate(ref _registerPropertyAction, dpmType.GetMethod("RegisterProperty"));
            CreateDelegate(ref _unregisterPropertyAction, dpmType.GetMethod("UnregisterProperty"));
        }

        static void CreateDelegate<T>(ref T action, MethodInfo methodInfo)
            where T : Delegate
        {
            action = (T)Delegate.CreateDelegate(typeof(T), methodInfo);
        }

        public interface IDriver: IDisposable
        {
            void RegisterProperties(Object target, params string[] propertyPaths);
            void UnregisterProperties(Object target, params string[] propertyPaths);
        }

        public class Driver : ScriptableObject, IDriver
        {
            private bool _disposed;

            public void Dispose()
            {
                if (_disposed)
                    return;

                _disposed = true;

                if (Application.isPlaying)
                    ScriptableObject.Destroy(this);
                else
                    ScriptableObject.DestroyImmediate(this);
            }

            public void RegisterProperties(Object target, params string[] propertyPaths)
            {
                foreach (var propertyPath in propertyPaths)
                    _registerPropertyAction(this, target, propertyPath);
            }

            public void UnregisterProperties(Object target, params string[] propertyPaths)
            {
                foreach (var propertyPath in propertyPaths)
                    _unregisterPropertyAction(this, target, propertyPath);
            }
        }

        public static IDriver CreateDriver()
        {
            return ScriptableObject.CreateInstance<Driver>();
        }

        public static RegistrationToken RegisterProperties(Object driver, Object target, params string[] propertyPaths)
        {
            foreach (var propertyPath in propertyPaths)
                _registerPropertyAction(driver, target, propertyPath);

            return new RegistrationToken(driver, target, propertyPaths.ToArray());
        }

        public static void UnregisterProperties(Object driver, Object target, params string[] propertyPaths)
        {
            foreach (var propertyPath in propertyPaths)
                _unregisterPropertyAction(driver, target, propertyPath);
        }

        public static RegistrationToken RegisterProperty(Object driver, Object target, string propertyPath)
        {
            _registerPropertyAction(driver, target, propertyPath);

            return new RegistrationToken(driver, target, new[] { propertyPath });
        }

        public static void UnregisterProperty(Object driver, Object target, string propertyPath)
        {
            _unregisterPropertyAction(driver, target, propertyPath);
        }

        public class RegistrationToken : IDisposable
        {
            private Object _driver;
            private Object _target;
            private string[] _propertyPaths;

            internal RegistrationToken(Object driver, Object target, string[] propertyPaths)
            {
                _driver = driver;
                _target = target;
                _propertyPaths = propertyPaths;
            }

            public void Dispose()
            {
                UnregisterProperties(_driver, _target, _propertyPaths);
            }
        }
    }
}