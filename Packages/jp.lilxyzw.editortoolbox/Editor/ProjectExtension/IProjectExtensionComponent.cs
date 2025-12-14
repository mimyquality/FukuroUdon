using System;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    public interface IProjectExtensionComponent
    {
        public int Priority { get; }
        public void OnGUI(ref Rect currentRect, string guid, string path, string name, string extension, Rect fullRect);
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExportsProjectExtensionComponent : Attribute
    {
        public Type[] Types { get; }
        public ExportsProjectExtensionComponent(params Type[] types) => Types = types;
    }
}
