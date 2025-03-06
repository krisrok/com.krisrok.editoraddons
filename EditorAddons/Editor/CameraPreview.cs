using UnityEngine;
using UnityEditor;

namespace EditorAddons.Editor
{
    class CameraPreview : EditorWindow
    {
        [SerializeField]
        private bool _autoSelectCamera = true;

        [SerializeField]
        private Camera _camera;

        [SerializeField]
        private bool _followGameViewResolution = true;

        [SerializeField]
        private Texture _overlayTexture;

        [SerializeField]
        private float _overlayAlpha = 1;

        private bool _isVisible;
        private RenderTexture _renderTexture;
        private Rect _cameraImageRect;

        private SerializedObject _serializedObject;
        private SerializedProperty _autoSelectCameraProperty;
        private SerializedProperty _cameraProperty;
        private SerializedProperty _overlayTextureProperty;
        private SerializedProperty _overlayAlphaProperty;
        private SerializedProperty _followGameViewResolutionProperty;

        [MenuItem("Tools/Camera Preview")]
        static void OpenWindow()
        {
            var editorWindow = GetWindow<CameraPreview>(typeof(CameraPreview));
            editorWindow.Show();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Camera Preview");

            _serializedObject = new SerializedObject(this);
            
            _autoSelectCameraProperty = _serializedObject.FindProperty(nameof(_autoSelectCamera));
            _cameraProperty = _serializedObject.FindProperty(nameof(_camera));
            _overlayTextureProperty = _serializedObject.FindProperty(nameof(_overlayTexture));
            _overlayAlphaProperty = _serializedObject.FindProperty(nameof(_overlayAlpha));
            _followGameViewResolutionProperty = _serializedObject.FindProperty(nameof(_followGameViewResolution));

            //_cameraProperty.objectReferenceValue = Camera.main;
            _camera = Camera.main;
            _serializedObject.Update();
        }

        void Update()
        {
            if (_camera != null && _isVisible)
            {
                EnsureRenderTexture();

                if (_renderTexture == null)
                    return;

                _camera.renderingPath = RenderingPath.UsePlayerSettings;
                var tmpTexture = _camera.targetTexture;
                _camera.targetTexture = _renderTexture;
                _camera.Render();
                _camera.targetTexture = tmpTexture;

                Repaint();
            }
        }

        private void OnFocus()
        {
            OnSelectionChange();
        }

        void OnBecameVisible()
        {
            _isVisible = true;

            OnSelectionChange();
        }

        void OnBecameInvisible()
        {
            _isVisible = false;
        }

        void OnSelectionChange()
        {
            if (_autoSelectCamera == false)
                return;

            var obj = Selection.activeGameObject;
            if (obj == null)
                return;

            var cam = obj.GetComponent<Camera>();
            if (cam == null)
                return;

            _camera = cam;
            _serializedObject.Update();
        }

        private void OnDestroy()
        {
            _serializedObject.Dispose();
            _autoSelectCameraProperty.Dispose();
            _cameraProperty.Dispose();
            _overlayTextureProperty.Dispose();
            _overlayAlphaProperty.Dispose();

            if (_renderTexture != null)
            {
                DestroyImmediate(_renderTexture);
            }
        }

        void EnsureRenderTexture()
        {
            var res = GetRenderResolution();

            if (_renderTexture == null
                || res.x != _renderTexture.width
                || res.y != _renderTexture.height)
            {
                if (res.x == 0 || res.y == 0)
                    return;

                _renderTexture = new RenderTexture(res.x, res.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            }
        }

        Vector2Int GetRenderResolution()
        {
            int width = 0;
            int height = 0;

            if (_followGameViewResolution)
            {
                var parts = UnityStats.screenRes.Split(new[] { 'x' });
                width = int.Parse(parts[0]);
                height = int.Parse(parts[1]);
            }

            if (width == 0 || height == 0)
            {
                width = (int)_cameraImageRect.width;
                height = (int)_cameraImageRect.height;
            }

            return new Vector2Int(width, height);
        }

        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_autoSelectCameraProperty);
            
            EditorGUI.BeginDisabledGroup(_autoSelectCamera);
            EditorGUILayout.ObjectField(_cameraProperty);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(_followGameViewResolutionProperty);

            EditorGUILayout.ObjectField(_overlayTextureProperty, GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight));
            if (_overlayTexture != null)
            {
                EditorGUILayout.Slider(_overlayAlphaProperty, 0f, 1f);
            }
            
            if (EditorGUI.EndChangeCheck())
                _serializedObject.ApplyModifiedProperties();

            DrawCameraImage();
        }

        private void DrawCameraImage()
        {
            _cameraImageRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();

            var res = GetRenderResolution();

            var rRatio = (float)res.x / res.y;
            var pRatio = _cameraImageRect.width / _cameraImageRect.height;

            var scale = new Vector2(pRatio > rRatio ? rRatio / pRatio : 1, pRatio > rRatio ? 1 : pRatio / rRatio);

            var rect = new Rect((1 - scale.x) * 0.5f * _cameraImageRect.width + _cameraImageRect.xMin, (1 - scale.y) * 0.5f * _cameraImageRect.height + _cameraImageRect.yMin, _cameraImageRect.width * scale.x, _cameraImageRect.height * scale.y);

            if (_renderTexture != null)
                GUI.DrawTexture(rect, _renderTexture);

            if (_overlayTexture != null)
                GUI.DrawTexture(rect, _overlayTexture, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: new Color(1, 1, 1, _overlayAlpha), borderWidth: 0, borderRadius: 0);
        }
    }
}