using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[assembly: UxmlNamespacePrefix("VRC.SDKBase.Editor.Elements", "vrc")]
namespace VRC.SDKBase.Editor.Elements
{
    public class ContentWarningsField : OptionsPopupField<string>
    {
        private static readonly string[] CONTENT_WARNING_TAGS = { "content_sex", "content_adult", "content_violence", "content_gore", "content_horror" };
        
        protected override IList<string> GetOptions()
        {
            return CONTENT_WARNING_TAGS;
        }

        protected override string GetOptionName(string option)
        {
            return option switch
            {
                "content_sex" => "Sexually Suggestive",
                "content_adult" => "Adult Language and Themes",
                "content_violence" => "Graphic Violence",
                "content_gore" => "Excessive Gore",
                "content_horror" => "Extreme Horror",
                _ => null
            };
        }

        protected override bool IsOptionLocked(string option)
        {
            return OriginalOptions.Contains("admin_content_reviewed") && OriginalOptions.Contains(option);
        }

        protected override int GetOptionCount()
        {
            return CONTENT_WARNING_TAGS.Length;
        }

        public new class UxmlFactory : UxmlFactory<ContentWarningsField, UxmlTraits> { }
        public new class UxmlTraits : OptionsPopupField<string>.UxmlTraits { }
    }
}