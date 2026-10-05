using System;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 프로젝트 문서를 기본 브라우저에서 연다.
/// </summary>
[InitializeOnLoad]
public static class DocumentationMenu
{
    private const string AUTO_OPEN_MENU = "Sample Project/시작 시 문서 자동 열기";
    private static string PreferenceKey => "FrameworkForUnity.Documentation.AutoOpen." + Application.dataPath;
    private static string SessionKey => PreferenceKey + ".StartupHandled";

    static DocumentationMenu()
    {
        if (Application.isBatchMode || SessionState.GetBool(SessionKey, false))
        {
            return;
        }

        // SessionState는 재컴파일 동안 유지되고 에디터 종료 시 초기화된다.
        SessionState.SetBool(SessionKey, true);
        EditorApplication.delayCall += OpenOnStartup;
    }

    /// <summary>
    /// 초기 임포트가 끝난 뒤 설정에 따라 문서를 한 번 연다.
    /// </summary>
    private static void OpenOnStartup()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            EditorApplication.delayCall += OpenOnStartup;
            return;
        }

        if (!EditorPrefs.GetBool(PreferenceKey, true))
        {
            return;
        }
        if (!File.Exists(DocumentPath))
        {
            return;
        }
        OpenDocumentation();
    }

    /// <summary>
    /// 이 프로젝트의 시작 시 문서 자동 열기 설정을 전환한다.
    /// </summary>
    [MenuItem(AUTO_OPEN_MENU, false, 101)]
    private static void ToggleAutoOpen()
    {
        bool isEnabled = !EditorPrefs.GetBool(PreferenceKey, true);
        EditorPrefs.SetBool(PreferenceKey, isEnabled);
        Menu.SetChecked(AUTO_OPEN_MENU, isEnabled);
    }

    /// <summary>
    /// 자동 열기 메뉴에 현재 설정을 체크 표시한다.
    /// </summary>
    [MenuItem(AUTO_OPEN_MENU, true)]
    private static bool ValidateAutoOpen()
    {
        Menu.SetChecked(AUTO_OPEN_MENU, EditorPrefs.GetBool(PreferenceKey, true));
        return true;
    }

    private static string DocumentPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), "docs", "documentation", "index.html");

    /// <summary>
    /// 프로젝트 경로를 기준으로 문서 홈을 연다.
    /// </summary>
    [MenuItem("Sample Project/프로젝트 문서 열기", false, 100)]
    private static void OpenDocumentation()
    {
        string documentPath = DocumentPath;

        if (!File.Exists(documentPath))
        {
            EditorUtility.DisplayDialog("프로젝트 문서", "문서 홈을 찾을 수 없습니다.\n\nnode docs/documentation/build.mjs\n명령으로 문서를 생성해 주세요.", "확인");
            return;
        }

        // 공백·한글이 포함된 경로도 열 수 있도록 파일 URI로 변환한다.
        Application.OpenURL(new Uri(documentPath).AbsoluteUri);
    }
}
