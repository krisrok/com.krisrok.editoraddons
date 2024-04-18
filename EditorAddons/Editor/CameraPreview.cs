using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;

namespace EditorAddons.Editor
{
    class CameraPreview : OdinEditorWindow
    {
        RenderTexture renderTexture;
        private bool _isVisible;

        [ShowInInspector]
        Camera _camera;

        [SerializeField]
        private Texture _overlayTexture;

        [MenuItem("Tools/Camera Preview")]
        static void OpenWindow()
        {
            var editorWindow = (EditorWindow)GetWindow<CameraPreview>(typeof(CameraPreview));
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
                _camera.targetTexture = renderTexture;
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
            var obj = Selection.activeGameObject;
            if (obj == null)
                return;

            var cam = obj.GetComponent<Camera>();
            if (cam == null)
                return;

            _camera = cam;
        }

        void EnsureRenderTexture()
        {
            var res = GetCurrentResolution();

            if (renderTexture == null
                || res.x != renderTexture.width
                || res.y != renderTexture.height)
            {
                renderTexture = new RenderTexture(res.x, res.y, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            }
        }

        Vector2Int GetCurrentResolution()
        {
            var parts = UnityStats.screenRes.Split(new[] { 'x' });
            var width = int.Parse(parts[0]);
            var height = int.Parse(parts[1]);

            return new Vector2Int(width, height);
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();

            var res = GetCurrentResolution();

            var rRatio = (float)res.x / res.y;
            var pRatio = position.width / position.height;

            var scale = new Vector2(pRatio > rRatio ? rRatio / pRatio : 1, pRatio > rRatio ? 1 : pRatio / rRatio);

            var rect = new Rect((1 - scale.x) * 0.5f * position.width, (1 - scale.y) * 0.5f * position.height, position.width * scale.x, position.height * scale.y);

            if (renderTexture != null)
                GUI.DrawTexture(rect, renderTexture);

            if (_overlayTexture != null)
            {
                GUI.DrawTexture(rect, _overlayTexture);
            }
        }
    }
}