using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Utility;

public static class DelegateRegistry
{
    private static readonly HashSet<Type> _registered = new();

    private static readonly MethodInfo ToDynValueGeneric = typeof(DelegateRegistry)
        .GetMethod(nameof(ToDynValue), BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo FromDynValueGeneric = typeof(DelegateRegistry)
        .GetMethod(nameof(FromDynValue), BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly MethodInfo ClosureCallMethod =
        typeof(Closure).GetMethod("Call", new[] { typeof(DynValue[]) });

    public static void RegisterDelegatesFromMethodsOf(Type type)
    {
        if (type == null) return;
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            foreach (var param in method.GetParameters())
            {
                RegisterDelegateType(param.ParameterType);
            }
        }
    }

    public static void RegisterDelegateType(Type delegateType)
    {
        if (delegateType == null) return;
        if (!typeof(Delegate).IsAssignableFrom(delegateType)) return;
        if (delegateType == typeof(Delegate) || delegateType == typeof(MulticastDelegate)) return;
        if (!_registered.Add(delegateType)) return;

        var invoke = delegateType.GetMethod("Invoke");
        if (invoke == null) return;
        if (invoke.GetParameters().Any(p => p.ParameterType.IsByRef || p.IsOut || p.ParameterType.IsPointer)) return;

        Script.GlobalOptions.CustomConverters.SetScriptToClrCustomConversion(
            DataType.Function, delegateType,
            dv => BuildWrapper(delegateType, dv.Function));
    }

    private static Delegate BuildWrapper(Type delegateType, Closure closure)
    {
        var invoke = delegateType.GetMethod("Invoke");
        var paramInfos = invoke.GetParameters();
        var returnType = invoke.ReturnType;

        var parameters = paramInfos
            .Select(p => Expression.Parameter(p.ParameterType, p.Name))
            .ToArray();

        var dynValueArgs = parameters
            .Select(p => (Expression)Expression.Call(ToDynValueGeneric.MakeGenericMethod(p.Type), p))
            .ToArray();

        var argsArray = Expression.NewArrayInit(typeof(DynValue), dynValueArgs);
        var closureConst = Expression.Constant(closure, typeof(Closure));
        var callExpr = Expression.Call(closureConst, ClosureCallMethod, argsArray);

        Expression body;
        if (returnType == typeof(void))
        {
            body = Expression.Block(typeof(void), callExpr);
        }
        else
        {
            body = Expression.Call(FromDynValueGeneric.MakeGenericMethod(returnType), callExpr);
        }

        return Expression.Lambda(delegateType, body, parameters).Compile();
    }

    private static DynValue ToDynValue<T>(T arg)
    {
        if (arg is DynValue dv) return dv;
        if (arg == null) return DynValue.Nil;

        var t = typeof(T);
        if (t == typeof(string)) return DynValue.NewString((string)(object)arg);
        if (t == typeof(bool)) return DynValue.NewBoolean((bool)(object)arg);
        if (t.IsPrimitive || t == typeof(decimal)) return DynValue.NewNumber(Convert.ToDouble(arg));
        if (t.IsEnum) return DynValue.NewNumber(Convert.ToDouble(arg));

        return MoonSharp.Interpreter.UserData.Create(arg);
    }

    private static R FromDynValue<R>(DynValue value)
    {
        if (typeof(R) == typeof(DynValue)) return (R)(object)value;
        if (value == null || value.IsNil() || value.IsVoid()) return default;

        var t = typeof(R);
        if (t == typeof(string))
            return (R)(object)(value.Type == DataType.String ? value.String : null);
        if (t == typeof(bool))
            return value.Type == DataType.Boolean ? (R)(object)value.Boolean : default;
        if (t.IsPrimitive || t == typeof(decimal))
            return value.Type == DataType.Number ? (R)Convert.ChangeType(value.Number, t) : default;
        if (t.IsEnum)
            return value.Type == DataType.Number ? (R)Enum.ToObject(t, (long)value.Number) : default;

        if (value.Type == DataType.UserData && value.UserData?.Object is R r) return r;
        return default;
    }
}
