using System.Linq;
using NUnit.Framework;
using UnityEngine.Networking;
using VRC.Udon.Editor;
using VRC.Udon.Editor.ProgramSources.UdonGraphProgram.UI;

namespace Tests
{

    public class GraphNodeTests
    {
        [Test]
        public void CheckHelpURLsForSystemNodes()
        {
            var systemRegistry = UdonEditorManager.Instance
                .GetTopRegistries()
                .First(r=>r.Key.Equals("System"))
                .Value;
            
            foreach (var keyValuePair in systemRegistry)
            {
                var registry = keyValuePair.Value;
                var definitions = registry.GetNodeDefinitions();
                foreach (var definition in definitions)
                {
                    // Skip some links which we hide away
                    if (!UdonGraphExtensions.ShouldShowDocumentationLink(definition)) continue;
                    
                    var link = UdonGraphExtensions.GetDocumentationLink(definition);
                    Assert.IsTrue(CheckUrlResolves(link), $"URL does not resolve for {definition.fullName}: {link}");
                }
            }
            Assert.Pass("All Systems UdonNodeDefinitions have valid links!");
        }
        
        private bool CheckUrlResolves(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                var operation = request.SendWebRequest();
                while (!operation.isDone) { }

                return request.result == UnityWebRequest.Result.Success;
            }
        }
    }

}