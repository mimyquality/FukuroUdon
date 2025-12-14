using System;
using UnityEngine.UIElements;

namespace jp.lilxyzw.editortoolbox
{
    public interface IToolbarExtensionComponent
    {
        public int Priority { get; }
        public bool InLeftSide { get; }
        public VisualElement GetRootElement();
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExportsToolbarExtensionComponent : Attribute
    {
        public Type[] Types { get; }
        public ExportsToolbarExtensionComponent(params Type[] types) => Types = types;
    }
}
