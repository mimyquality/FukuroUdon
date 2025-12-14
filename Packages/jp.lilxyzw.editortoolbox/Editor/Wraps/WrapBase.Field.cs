using System;
using System.Linq.Expressions;
using System.Reflection;

namespace jp.lilxyzw.editortoolbox
{
    internal abstract partial class WrapBase
    {
        protected static (Func<R> g, Action<R> s) GetField<R>(Type type, string name)
        {
            var expR = Expression.Parameter(typeof(R));
            var (get, set) = GetFieldBase(type, name, null, expR);
            var getL = Expression.Lambda<Func<R>>(get).Compile();
            var setL = Expression.Lambda<Action<R>>(set, expR).Compile();
            return (getL, setL);
        }

        protected static (Func<object> g, Delegate s) GetField(Type type, string name, Type typeR)
        {
            var expR = Expression.Parameter(typeR);
            var (get, set) = GetFieldBase(type, name, null, expR);
            var getL = Expression.Lambda<Func<object>>(get).Compile();
            var setL = Expression.Lambda(set, expR).Compile();
            return (getL, setL);
        }

        protected static (Delegate g, Delegate s) GetFieldIns(Type type, string name, Type typeR)
        {
            var expT = Expression.Parameter(type);
            var expR = Expression.Parameter(typeR);
            var (get, set) = GetFieldBase(type, name, expT, expR);
            var getL = Expression.Lambda(get, expT).Compile();
            var setL = Expression.Lambda(set, expT, expR).Compile();
            return (getL, setL);
        }

        // Base
        private static (MemberExpression get, BinaryExpression set) GetFieldBase(Type type, string name, ParameterExpression expT, ParameterExpression expR)
        {
            var fi = type.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var get = Expression.Field(expT, fi);
            var set = Expression.Assign(get, expR);
            return (get, set);
        }
    }
}
