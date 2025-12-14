using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace jp.lilxyzw.editortoolbox
{
    internal abstract partial class WrapBase
    {
        // Action
        protected static Action GetAction(Type type, string name)
        {
            var call = GetMethodBase(type, name);
            return Expression.Lambda<Action>(call).Compile();
        }

        protected static Action<T1> GetAction<T1>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1)});
            return Expression.Lambda<Action<T1>>(call, exps).Compile();
        }

        protected static Action<T1,T2> GetAction<T1,T2>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2)});
            return Expression.Lambda<Action<T1,T2>>(call, exps).Compile();
        }

        protected static Action<T1,T2,T3> GetAction<T1,T2,T3>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2),typeof(T3)});
            return Expression.Lambda<Action<T1,T2,T3>>(call, exps).Compile();
        }

        protected static Action<T1,T2,T3,T4> GetAction<T1,T2,T3,T4>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2),typeof(T3),typeof(T4)});
            return Expression.Lambda<Action<T1,T2,T3,T4>>(call, exps).Compile();
        }

        protected static Action<T1,T2,T3,T4,T5> GetAction<T1,T2,T3,T4,T5>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5)});
            return Expression.Lambda<Action<T1,T2,T3,T4,T5>>(call, exps).Compile();
        }

        protected static Action<T1,T2,T3,T4,T5,T6> GetAction<T1,T2,T3,T4,T5,T6>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2),typeof(T3),typeof(T4),typeof(T5),typeof(T6)});
            return Expression.Lambda<Action<T1,T2,T3,T4,T5,T6>>(call, exps).Compile();
        }

        // Func
        protected static Func<R> GetFunc<R>(Type type, string name)
        {
            var call = GetMethodBase(type, name);
            return Expression.Lambda<Func<R>>(call).Compile();
        }

        protected static Func<T1,R> GetFunc<T1,R>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1)});
            return Expression.Lambda<Func<T1,R>>(call, exps).Compile();
        }

        protected static Func<T1,T2,R> GetFunc<T1,T2,R>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2)});
            return Expression.Lambda<Func<T1,T2,R>>(call, exps).Compile();
        }

        protected static Func<T1,T2,T3,R> GetFunc<T1,T2,T3,R>(Type type, string name)
        {
            var (call, exps) = GetMethodBase(type, name, new[]{typeof(T1),typeof(T2),typeof(T3)});
            return Expression.Lambda<Func<T1,T2,T3,R>>(call, exps).Compile();
        }

        // Base
        private static MethodCallExpression GetMethodBase(Type type, string name)
        {
            var mi = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            var call = Expression.Call(mi);
            return call;
        }

        private static (MethodCallExpression call, ParameterExpression[] exps) GetMethodBase(Type type, string name, Type[] args)
        {
            var exps = args.Select(a => Expression.Parameter(a)).ToArray();
            var mi = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, args, new[]{new ParameterModifier(args.Length)});

            var call = Expression.Call(mi, exps);
            return (call, exps);
        }
    }
}
