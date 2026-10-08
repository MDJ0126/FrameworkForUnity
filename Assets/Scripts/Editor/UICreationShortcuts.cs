using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

/// <summary>
/// UI 오브젝트 생성 단축키를 유니티 기본 생성 메뉴에 연결한다.
/// </summary>
public static class UICreationShortcuts
{
    /// <summary>
    /// RectTransform과 Image를 포함하는 UI 오브젝트를 생성한다.
    /// </summary>
    [Shortcut("Sample Project/Create UI Image", KeyCode.I, ShortcutModifiers.Control | ShortcutModifiers.Shift)]
    private static void CreateImage()
    {
        EditorApplication.ExecuteMenuItem("GameObject/UI/Image");
    }

    /// <summary>
    /// RectTransform과 TextMeshProUGUI를 포함하는 UI 오브젝트를 생성한다.
    /// </summary>
    [Shortcut("Sample Project/Create UI TMP Text", KeyCode.T, ShortcutModifiers.Control | ShortcutModifiers.Shift)]
    private static void CreateText()
    {
        EditorApplication.ExecuteMenuItem("GameObject/UI/Text - TextMeshPro");
    }
}
