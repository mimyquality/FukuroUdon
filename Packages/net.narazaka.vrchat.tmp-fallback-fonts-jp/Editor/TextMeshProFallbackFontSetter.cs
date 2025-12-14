using TMPro;
using UnityEditor;

public class TextMeshProFallbackFontSetter
{
    static string DefaultFontAssetGUID = "b0cf90c18247f154094021e2de9bf529";
    static string FallbackFontAssetGUID = "32134e5dc8c950c4cb5bb7deaae7d539";
    static TMP_FontAsset DefaultFontAsset => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(DefaultFontAssetGUID));
    static TMP_FontAsset FallbackFontAsset => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(FallbackFontAssetGUID));

    [MenuItem("Tools/TextMesh Pro VRC Fallback Font JPを設定")]
    static void SetFallbackFont()
    {
        var setting = TMP_Settings.instance;
        if (setting == null)
        {
            EditorUtility.DisplayDialog("Error", "TextMeshProの設定がありません！\n「Edit→Project Settings」でProject Settingsを開き、TextMesh ProタブからTMP Essentialsを先にインポートして下さい。", "OK");
            return;
        }
        if (DefaultFontAsset == null || FallbackFontAsset == null)
        {
            EditorUtility.DisplayDialog("Error", "フォントアセットが見つかりません。\nTextMesh Pro VRC Fallback Font JPのインストールが壊れている可能性があるため、再インストールなどを試して下さい。", "OK");
            return;
        }
        var so = new SerializedObject(setting);
        so.Update();
        so.FindProperty("m_defaultFontAsset").objectReferenceValue = DefaultFontAsset;
        var fallbackFontAssets = so.FindProperty("m_fallbackFontAssets");
        fallbackFontAssets.arraySize = 1;
        fallbackFontAssets.GetArrayElementAtIndex(0).objectReferenceValue = FallbackFontAsset;
        so.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }
}
