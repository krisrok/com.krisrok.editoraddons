﻿using UnityEngine;
using UnityEditor;

namespace EditorAddons.Editor
{
    class CameraPreview : EditorWindow
    {

        //[ShowInInspector]
        private bool _autoSelectCamera = true;

        //[ShowInInspector]
        //[DisableIf(nameof(_autoSelectCamera))]
        private Camera _camera;

        private Texture _overlayTexture;
        private float _overlayAlpha = 1;

        private bool _isVisible;
        private RenderTexture _renderTexture;

        [MenuItem("Tools/Camera Preview")]
        static void OpenWindow()
        {
            var editorWindow = GetWindow<CameraPreview>(typeof(CameraPreview));
            //editorWindow.autoRepaintOnSceneChange = true;
            editorWindow.titleContent = new GUIContent("Camera Preview");
            editorWindow.Show();
        }

        void Awake()
        {
            _camera = Camera.main;
        }

        void Update()
        {
            if (_camera != null && _isVisible)
            {
                EnsureRenderTexture();

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
        }

        private void OnDestroy()
        {
            if (_renderTexture == null)
                return;

            DestroyImmediate(_renderTexture);
        }

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();

        //    if (_renderTexture == null)
        //        return;

        //    DestroyImmediate(_renderTexture);
        //}

        void EnsureRenderTexture()
        {
            var res = GetGameViewResolution();

            if (_renderTexture == null
                || res.x != _renderTexture.width
                || res.y != _renderTexture.height)
            {
                _renderTexture = new RenderTexture(res.x, res.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            }
        }

        Vector2Int GetGameViewResolution()
        {
            var parts = UnityStats.screenRes.Split(new[] { 'x' });
            var width = int.Parse(parts[0]);
            var height = int.Parse(parts[1]);

            return new Vector2Int(width, height);
        }

        private static GUIContent _autoSelectCameraLabel = new GUIContent("Auto select camera");
        private static GUIContent _cameraLabel = new GUIContent("Camera");
        private static GUIContent _overlayTextureLabel = new GUIContent("Overlay texture");
        private static GUIContent _overlayAlphaLabel = new GUIContent("Overlay alpha");

        private void OnGUI()
        {
            _autoSelectCamera = EditorGUILayout.Toggle(_autoSelectCameraLabel, _autoSelectCamera);
            EditorGUI.BeginDisabledGroup(_autoSelectCamera);
            _camera = EditorGUILayout.ObjectField(_cameraLabel, _camera, typeof(Camera), allowSceneObjects: true) as Camera;
            EditorGUI.EndDisabledGroup();

            _overlayTexture = EditorGUILayout.ObjectField(_overlayTextureLabel, _overlayTexture, typeof(Texture), allowSceneObjects: false) as Texture;
            if(_overlayTexture != null)
            {
                _overlayAlpha = EditorGUILayout.Slider(_overlayAlphaLabel, _overlayAlpha, 0f, 1f);
            }

            var position = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            EditorGUILayout.EndVertical();

            var res = GetGameViewResolution();

            var rRatio = (float)res.x / res.y;
            var pRatio = position.width / position.height;

            var scale = new Vector2(pRatio > rRatio ? rRatio / pRatio : 1, pRatio > rRatio ? 1 : pRatio / rRatio);

            var rect = new Rect((1 - scale.x) * 0.5f * position.width + position.xMin, (1 - scale.y) * 0.5f * position.height + position.yMin, position.width * scale.x, position.height * scale.y);

            if (_renderTexture != null)
                GUI.DrawTexture(rect, _renderTexture);

            if (_overlayTexture != null)
                GUI.DrawTexture(rect, _overlayTexture, ScaleMode.StretchToFill, alphaBlend: true, imageAspect: 0, color: new Color(1, 1, 1, _overlayAlpha), borderWidth: 0, borderRadius: 0);
        }
        //protected override void OnEndDrawEditors()
        //{
        //    base.OnEndDrawEditors();

        //    var position = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        //    EditorGUILayout.EndVertical();

        //    var res = GetGameViewResolution();

        //    var rRatio = (float)res.x / res.y;
        //    var pRatio = position.width / position.height;

        //    var scale = new Vector2(pRatio > rRatio ? rRatio / pRatio : 1, pRatio > rRatio ? 1 : pRatio / rRatio);

        //    var rect = new Rect((1 - scale.x) * 0.5f * position.width + position.xMin, (1 - scale.y) * 0.5f * position.height + position.yMin, position.width * scale.x, position.height * scale.y);

        //    if (_renderTexture != null)
        //        GUI.DrawTexture(rect, _renderTexture);

        //    if (_overlayTexture != null)
        //        GUI.DrawTexture(rect, _overlayTexture);
        //}
    }
}