using System;
using System.Linq.Expressions;
using System.Reflection;

namespace jp.lilxyzw.editortoolbox
{
    internal abstract partial class WrapBase
    {
        protected static (Func<R> g, Action<R> s) GetProperty<R>(Type type, string name)
        {
            var expR = Expression.Parameter(typeof(R));
            var (get, set) = GetPropertyBase(type, name, null, expR);
            var getL = Expression.Lambda<Func<R>>(get).Compile();
            var setL = Expression.Lambda<Action<R>>(set, expR).Compile();
            return (getL, setL);
        }

        protected static (Func<object> g, Delegate s) GetProperty(Type type, string name, Type typeR)
        {
            var expR = Expression.Parameter(typeR);
            var (get, set) = GetPropertyBase(type, name, null, expR);
            var getL = Expression.Lambda<Func<object>>(get).Compile();
            var setL = Expression.Lambda(set, expR).Compile();
            return (getL, setL);
        }

        protected static (Delegate g, Delegate s) GetPropertyIns(Type type, string name, Type typeR)
        {
            var expT = Expression.Parameter(type);
            var expR = Expression.Parameter(typeR);
            var (get, set) = GetPropertyBase(type, name, expT, expR);
            var getL = Expression.Lambda(get, expT).Compile();
            var setL = Expression.Lambda(set, expT, expR).Compile();
            return (getL, setL);
        }

        // Base
        private static (MemberExpression get, BinaryExpression set) GetPropertyBase(Type type, string name, ParameterExpression expT, ParameterExpression expR)
        {
            var pi = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var get = Expression.Property(expT, pi);
            var set = Expression.Assign(get, expR);
            return (get, set);
        }
    }
}
