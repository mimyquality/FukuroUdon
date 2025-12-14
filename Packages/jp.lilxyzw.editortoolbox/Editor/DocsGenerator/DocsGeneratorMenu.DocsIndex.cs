using System;
using System.IO;
using System.Linq;
using System.Text;

namespace jp.lilxyzw.editortoolbox
{
    internal static partial class DocsGeneratorMenu
    {
        private static void BuildDocsIndex(string root, string code, Func<string,string> loc)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"# {loc("Documents")}");
            sb.AppendLine();
            sb.AppendLine(loc("This is the documentation page for lilEditorToolbox."));
            sb.AppendLine();
            sb.AppendLine($"<div class=\"table-of-contents\">");
            sb.AppendLine($"    <ul>");

            // 設定は先に表示
            {
            var title = File.ReadLines(root+"/docs/Settings.md").First().Substring(2);
            sb.AppendLine($"    <li><a href=\"./Settings\">{title}</a></li>");
            }

            // 通常のクラス
            foreach(var file in Directory.GetFiles(root+"/docs", "*.md", SearchOption.TopDirectoryOnly))
            {
            if(file.EndsWith("index.md") || file.EndsWith("Settings.md")) continue;
            var title = File.ReadLines(file).First().Substring(2);
            sb.AppendLine($"    <li><a href=\"./{Path.GetFileNameWithoutExtension(file)}\">{title}</a></li>");
            }

            // EditorWindow
            sb.AppendLine($"    <li><p>{loc("EditorWindow")}</p>");
            sb.AppendLine($"        <ul>");
            foreach(var file in Directory.GetFiles(root+"/docs/EditorWindow", "*.md", SearchOption.TopDirectoryOnly))
            {
            if(file.EndsWith("index.md")) continue;
            var title = File.ReadLines(file).First().Substring(2);
            sb.AppendLine($"            <li><a href=\"./EditorWindow/{Path.GetFileNameWithoutExtension(file)}\">{title}</a></li>");
            }
            sb.AppendLine($"        </ul>");
            sb.AppendLine($"    </li>");

            // Components
            sb.AppendLine($"    <li><p>{loc("Components")}</p>");
            sb.AppendLine($"        <ul>");
            foreach(var file in Directory.GetFiles(root+"/docs/Components", "*.md", SearchOption.TopDirectoryOnly))
            {
            if(file.EndsWith("index.md")) continue;
            var title = File.ReadLines(file).First().Substring(2);
            sb.AppendLine($"            <li><a href=\"./Components/{Path.GetFileNameWithoutExtension(file)}\">{title}</a></li>");
            }
            sb.AppendLine($"        </ul>");
            sb.AppendLine($"    </li>");

            sb.AppendLine($"    </ul>");
            sb.AppendLine($"</div>");

            WriteText($"{root}/docs/index.md", sb.ToString());
        }
    }
}
