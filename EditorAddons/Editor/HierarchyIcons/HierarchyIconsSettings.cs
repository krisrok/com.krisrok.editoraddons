#if UNITY_EDITOR
using UnityEngine;
#if EDITORADDONS_SERIALIZABLESETTINGS
using SerializableSettings;
using SerializableSettings.Editor;

[EditorUserSettings("Hierarchy Icons ⁂")]
public class HierarchyIconsSettings : SerializableSettings<HierarchyIconsSettings>
{
    // Settings defined here can be accessed during runtime and in the Unity Editor.
    // Make sure to keep the #if UNITY_EDITOR directives or put the script into an Editor-only assembly.
    // Access the settings via Edit/Preferences/... and via code: HierarchyIconsSettings.Instance.
    // These settings can be different on a per-user (or per-developer) basis. Think favourite editor color.
    // Please exclude the Assets/Settings/Editor/User/ folder in e.g. your .gitignore file.
#else
public class HierarchyIconsSettings : ScriptableObject
{
    private static HierarchyIconsSettings _instance;
    public static HierarchyIconsSettings Instance => _instance ??= CreateInstance<HierarchyIconsSettings>();
#endif

    [field: Header("Default drawers")]
    [field: SerializeField]
    public bool DrawObjectIcon { get; private set; }

    [field: SerializeField]
    public bool DrawActiveToggle { get; private set; } = true;
}
#endif