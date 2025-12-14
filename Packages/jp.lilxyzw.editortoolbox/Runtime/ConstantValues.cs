namespace jp.lilxyzw.editortoolbox.runtime
{
    // URLやGUIDなどの定数はここに集約して、変更があった場合に更新しやすくします。
    internal class ConstantValues
    {
        internal const string TOOL_NAME = "lilEditorToolbox";
        internal const string PACKAGE_NAME = "editortoolbox";
        internal const string PACKAGE_NAME_FULL = "jp.lilxyzw." + PACKAGE_NAME;
        internal const string COMPONENTS_BASE = TOOL_NAME + "/LE ";
        internal const string URL_DOCS_BASE = "https://lilxyzw.github.io/lilEditorToolbox/redirect#";
        internal const string URL_DOCS_COMPONENT = URL_DOCS_BASE + "docs/Components/";
    }
}
