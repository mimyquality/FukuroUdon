using System;
using System.IO;
using System.Text;

namespace jp.lilxyzw.editortoolbox
{
    internal static partial class DocsGeneratorMenu
    {
        private static void BuildHome(string root, string code, Func<string,string> loc)
        {
            var sb = new StringBuilder();
            sb.Append(File.ReadAllText("docs_base/home.md"));
            sb.Replace("TOOLNAME", "lilEditorToolbox");
            sb.Replace("LANG", code);
            sb.Replace("TEXT", loc("Useful tool box"));
            sb.Replace("TAGLINE", loc("This package contains various useful editor extensions."));
            sb.Replace("ADDTOVCC", loc("Add to VCC"));
            sb.Replace("DOCUMENT", loc("Document"));
            sb.Replace("BOOTHLINK", "https://lilxyzw.booth.pm/items/6166069");

            WriteText($"{root}/index.md", sb.ToString());
            if(code == "ja_JP") WriteText($"docs/index.md", sb.ToString());
        }
    }
}
