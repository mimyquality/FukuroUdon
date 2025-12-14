using System;

namespace jp.lilxyzw.editortoolbox
{
    internal abstract partial class WrapBase
    {
        protected static Type MakeGeneric<T>(Type type)
        {
            return typeof(T).MakeGenericType(type);
        }
    }
}
