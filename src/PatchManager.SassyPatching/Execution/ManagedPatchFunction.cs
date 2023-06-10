using System.Collections;
using System.Reflection;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Exceptions;

namespace PatchManager.SassyPatching.Execution;

internal class ManagedPatchFunction : PatchFunction
{
    // private Func<Environment, List<PatchArgument>,Value> _execute;
    private MethodInfo _info;

    public ManagedPatchFunction(MethodInfo info)
    {
        _info = info;
    }

    private static object ConvertParameterValue(DataValue v, Type t)
    {
        if (t == typeof(string))
        {
            return v.String;
        }

        if (t == typeof(Dictionary<string, DataValue>))
        {
            return v.Dictionary;
        }

        if (t == typeof(List<DataValue>))
        {
            return v.List;
        }

        if (t == typeof(DataValue[]))
        {
            return v.List.ToArray();
        }

        if (t == typeof(byte))
        {
            return (byte)v.Number;
        }

        if (t == typeof(sbyte))
        {
            return (sbyte)v.Number;
        }

        if (t == typeof(short))
        {
            return (short)v.Number;
        }

        if (t == typeof(ushort))
        {
            return (ushort)v.Number;
        }

        if (t == typeof(int))
        {
            return (int)v.Number;
        }

        if (t == typeof(uint))
        {
            return (uint)v.Number;
        }

        if (t == typeof(long))
        {
            return (long)v.Number;
        }

        if (t == typeof(ulong))
        {
            return (ulong)v.Number;
        }

        if (t == typeof(float))
        {
            return (float)v.Number;
        }

        if (t == typeof(double))
        {
            return v.Number;
        }

        if (t == typeof(bool))
        {
            return v.Boolean;
        }

        if (v.Type == DataValue.DataType.None)
        {
            return null;
        }

        if (ConvertValueToSingleRankArray(v, t, out var singleRankArray)) return singleRankArray;

        if (ConvertValueToMultiRankArray(v, t, out var multiRankArray)) return multiRankArray;

        if (ConvertValueToList(v, t, out var list)) return list;
        
        if (ConvertValueToDictionary(v, t, out var convertParameterValue)) return convertParameterValue;
        
        var obj = Activator.CreateInstance(t);
        var dictionary = v.Dictionary;
        foreach (var field in t.GetFields())
        {
            field.SetValue(obj, ConvertParameterValue(dictionary[field.Name],field.FieldType));
        }

        return obj;
    }

    private static bool ConvertValueToDictionary(DataValue dataValue, Type type, out object dictionary)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            if (keyType != typeof(string))
            {
                throw new TypeConversionException("Value", type.Name);
            }

            var id = (IDictionary)Activator.CreateInstance(type);
            var dict = dataValue.Dictionary;
            foreach (var kv in dict)
            {
                id[kv.Key] = ConvertParameterValue(kv.Value, valueType);
            }

            {
                dictionary = id;
                return true;
            }
        }

        dictionary = null;
        return false;
    }

    private static bool ConvertValueToList(DataValue dataValue, Type type, out object list)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var valueType = type.GetGenericArguments()[0];
            var il = (IList)Activator.CreateInstance(type);
            var tab = dataValue.List;
            foreach (var x in tab)
            {
                il.Add(ConvertParameterValue(x, valueType));
            }

            {
                list = il;
                return true;
            }
        }

        list = null;
        return false;
    }

    private static bool ConvertValueToMultiRankArray(DataValue dataValue, Type type, out object multiRankArray)
    {
        if (type.IsArray)
        {
            var tab = dataValue.List;
            var rank = type.GetArrayRank();
            var dimensions = new int[rank];
            var dimCounter = tab;
            for (var i = 0; i < rank; i++)
            {
                dimensions[i] = dimCounter.Count;
                if (i != rank - 1)
                {
                    dimCounter = dimCounter[0].List;
                }
            }

            var arr = Array.CreateInstance(type.GetElementType()!, dimensions);

            void BuildArray(Array array, List<DataValue> data, params int[] idx)
            {
                var idxCopy = new int[idx.Length + 1];
                idx.CopyTo(idxCopy, 0);
                if (idx.Length == rank - 1)
                {
                    for (var i = 0; i < array.GetLength(idx.Length); i++)
                    {
                        idxCopy[idx.Length] = i;
                        array.SetValue(ConvertParameterValue(tab[i], type.GetElementType()), idxCopy);
                    }
                }
                else
                {
                    for (var i = 0; i < array.GetLength(idx.Length); i++)
                    {
                        idxCopy[idx.Length] = i;
                        BuildArray(array, data[i].List, idxCopy);
                    }
                }
            }

            BuildArray(arr, tab);
            {
                multiRankArray = arr;
                return true;
            }
        }

        multiRankArray = null;
        return false;
    }

    private static bool ConvertValueToSingleRankArray(DataValue dataValue, Type type, out object singleRankArray)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            var tab = dataValue.List;
            var arr = Array.CreateInstance(type.GetElementType()!, tab.Count);
            for (var i = 0; i < tab.Count; i++)
            {
                arr.SetValue(ConvertParameterValue(tab[i], type.GetElementType()), i);
            }

            {
                singleRankArray = arr;
                return true;
            }
        }

        singleRankArray = null;
        return false;
    }

    private static object CheckParameter(ParameterInfo info, DataValue argument)
    {
        var obj = ConvertParameterValue(argument, info.ParameterType);
        return obj;
    }

    private static DataValue ConvertReturnedValue(object value)
    {
        switch (value)
        {
            case null:
                return new DataValue(DataValue.DataType.None);
            case DataValue v:
                return v;
            case bool b:
                return b;
            case byte bv:
                return bv;
            case sbyte sbv:
                return sbv;
            case short sv:
                return sv;
            case ushort usv:
                return usv;
            case int iv:
                return iv;
            case uint uiv:
                return uiv;
            case long slv:
                return slv;
            case ulong ulv:
                return ulv;
            case float f:
                return f;
            case double d:
                return d;
            case string s:
                return s;
            case List<DataValue> lv:
                return lv;
            case DataValue[] av:
                return av.ToList();
            case Dictionary<string, DataValue> dv:
                return dv;
        }

        var t = value.GetType();
        if (ConvertSingleRankArrayToValue(value, t, out var singleRankArray)) return singleRankArray;
        if (ConvertMultiRankArrayToValue(value, t, out var multiRankArrayValue)) return multiRankArrayValue;
        if (ConvertGenericListOrDictionaryToValue(value, t, out var listOrDictionaryValue)) return listOrDictionaryValue;
        Dictionary<string, DataValue> objectData = new();


        // Only store public data
        foreach (var field in t.GetFields())
        {
            objectData[field.Name] = ConvertReturnedValue(field.GetValue(value));
        }

        return objectData;
    }

    private static bool ConvertGenericListOrDictionaryToValue(object value, Type type, out DataValue listOrDictionaryDataValue)
    {
        switch (type.IsGenericType)
        {
            case true when type.GetGenericTypeDefinition() == typeof(Dictionary<,>):
            {
                var keyType = type.GetGenericArguments()[0];
                if (keyType != typeof(string))
                {
                    throw new TypeConversionException(type.FullName, "Value");
                }

                var dict = (IDictionary)value;
                Dictionary<string, DataValue> newData = new();
                foreach (var k in dict.Keys)
                {
                    var ks = (string)k;
                    newData[ks] = ConvertReturnedValue(dict[k]);
                }

                {
                    listOrDictionaryDataValue = newData;
                    return true;
                }
            }
            case true when type.GetGenericTypeDefinition() == typeof(List<>):
            {
                listOrDictionaryDataValue = ((from object v in (IList)value select ConvertReturnedValue(v)).ToList());
                return true;
            }
        }

        listOrDictionaryDataValue = null;
        return false;
    }

    private static bool ConvertMultiRankArrayToValue(object value, Type t, out DataValue multiRankArrayDataValue)
    {
        if (t.IsArray)
        {
            var a = (Array)value;
            var rank = a.Rank;
            var outermostDimension = new List<DataValue>();

            void TraverseRank(List<DataValue> containingDimension, Array arr, int r, params int[] idx)
            {
                var size = a.GetLength(r);
                var idxCopy = new int[idx.Length + 1];
                idx.CopyTo(idxCopy, 0);
                if (r == a.Rank - 1)
                {
                    for (var i = 0; i < size; i++)
                    {
                        idxCopy[idx.Length] = i;
                        var v = arr.GetValue(idxCopy);
                        containingDimension.Add(ConvertReturnedValue(v));
                    }
                }
                else
                {
                    for (var i = 0; i < size; i++)
                    {
                        idxCopy[idx.Length] = i;
                        var currentDimension = new List<DataValue>();
                        TraverseRank(currentDimension, arr, r + 1, idxCopy);
                        outermostDimension.Add(currentDimension);
                    }
                }
            }


            TraverseRank(outermostDimension, a, 0);
            {
                multiRankArrayDataValue = outermostDimension;
                return true;
            }
        }

        multiRankArrayDataValue = null;
        return false;
    }

    private static bool ConvertSingleRankArrayToValue(object value, Type type, out DataValue singleRankArray)
    {
        if (type.IsArray && type.GetArrayRank() == 1)
        {
            var arr = (Array)value;
            List<DataValue> vs = new List<DataValue>();
            foreach (var obj in arr)
            {
                vs.Add(ConvertReturnedValue(obj));
            }

            {
                singleRankArray = vs;
                return true;
            }
        }

        singleRankArray = null;
        return false;
    }

    public override DataValue Execute(Environment env, List<PatchArgument> arguments)
    {
        List<object> args = new();

        // We are going to consume arguments as we go to prevent errors
        
        foreach (var parameter in _info.GetParameters())
        {
            if (parameter.ParameterType == typeof(Environment))
            {
                args.Add(env);
            }

            if (parameter.ParameterType == typeof(GlobalEnvironment))
            {
                args.Add(env.GlobalEnvironment);
            }

            if (parameter.ParameterType == typeof(Universe))
            {
                args.Add(env.GlobalEnvironment.Universe);
            }
            
            if (parameter.ParameterType == typeof(List<DataValue>) &&
                parameter.GetCustomAttributes().OfType<VarArgsAttribute>().Any())
            {
                var varArgs = new List<DataValue>();
                var remove = new List<int>();
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].ArgumentName != null) continue;
                    varArgs.Add(arguments[i].ArgumentDataValue);
                    remove.Add(i);
                }

                args.Add(varArgs);
                for (var i = remove.Count - 1; i >= 0; i--)
                {
                    arguments.RemoveAt(i);
                }
            }

            if (parameter.ParameterType == typeof(Dictionary<string, DataValue>) &&
                parameter.GetCustomAttributes().OfType<VarArgsAttribute>().Any())
            {
                var varArgs = new Dictionary<string, DataValue>();
                var remove = new List<int>();
                for (var i = 0; i < arguments.Count; i++)
                {
                    if (arguments[i].ArgumentName == null) continue;
                    varArgs.Add(arguments[i].ArgumentName, arguments[i].ArgumentDataValue);
                    remove.Add(i);
                }

                args.Add(varArgs);
                for (var i = remove.Count - 1; i >= 0; i--)
                {
                    arguments.RemoveAt(i);
                }
            }

            bool foundPositional = false;
            DataValue argument = null;
            int removalIndex = -1;
            for (int i = 0; i < arguments.Count; i++)
            {
                if (!foundPositional && arguments[i].ArgumentName == null)
                {
                    foundPositional = true;
                    removalIndex = i;
                    argument = arguments[i].ArgumentDataValue;
                }

                if (arguments[i].ArgumentName != parameter.Name) continue;
                removalIndex = i;
                argument = arguments[i].ArgumentDataValue;
                break;
            }

            if (removalIndex >= 0)
            {
                arguments.RemoveAt(removalIndex);
            }

            if (argument == null)
            {
                if (parameter.HasDefaultValue)
                {
                    args.Add(parameter.DefaultValue);
                }
                else
                {
                    throw new InvocationException($"No value found for argument: {parameter.Name}");
                }
            }
            else
            {
                args.Add(CheckParameter(parameter, argument));
            }

        }

        if (arguments.Count > 0)
        {
            throw new InvocationException("Too many arguments passed");
        }
        
        return ConvertReturnedValue(_info.Invoke(null, args.ToArray()));
    }
}