using System;
using UnityEngine;

namespace jp.lilxyzw.editortoolbox
{
    public interface IHierarchyExtensionComponent
    {
        /// <summary>
        /// <para>-2000 to -1001 : Background</para>
        /// <para>-1000 to -1 : No width decoration</para>
        /// <para>0 to 999 : Monospaced decoration</para>
        /// <para>1000 to 1999 : Variable width decoration</para>
        /// </summary>
        /// <value></value>
        public int Priority { get; }
        public void OnGUI(ref Rect currentRect, GameObject gameObject, int instanceID, Rect fullRect);
    }

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class ExportsHierarchyExtensionComponent : Attribute
    {
        public Type[] Types { get; }
        public ExportsHierarchyExtensionComponent(params Type[] types) => Types = types;
    }
}
