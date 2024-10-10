using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorAddons.Editor
{
    class OpenPackageManifestShortcut 
    {
        [MenuItem("Assets/Open manifest.json")]
        private static void OpenPackageManifest()
        {
            Process.Start(Path.Combine(Application.dataPath, "..", "Packages", "manifest.json"));
        }
    }
}
