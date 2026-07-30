using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PatchManager.PrefabPatching;

namespace PatchManager.LuaPatching.Builtin;

/// <summary>
/// Lua frontend for the public declarative prefab-patch schema. Lua tables are
/// converted directly into the same model used by C# and visual authoring.
/// </summary>
[MoonSharpUserData]
public sealed class PrefabPatchLuaBuilder
{
    private readonly PrefabPatchBuilder _builder;

    internal PrefabPatchLuaBuilder(PrefabPatchBuilder builder)
    {
        _builder = builder;
    }

    public PrefabPatchLuaBuilder Early()
    {
        _builder.Early();
        return this;
    }

    public PrefabPatchLuaBuilder Late()
    {
        _builder.Late();
        return this;
    }

    public PrefabPatchLuaBuilder First()
    {
        _builder.First();
        return this;
    }

    public PrefabPatchLuaBuilder Last()
    {
        _builder.Last();
        return this;
    }

    public PrefabPatchLuaBuilder Needs(params string[] ids)
    {
        _builder.NeedsMod(ids);
        return this;
    }

    public PrefabPatchLuaBuilder Conflicts(params string[] ids)
    {
        _builder.ConflictsMod(ids);
        return this;
    }

    public PrefabPatchLuaBuilder NeedsPatch(params string[] ids)
    {
        _builder.NeedsPatch(ids);
        return this;
    }

    public PrefabPatchLuaBuilder ConflictsPatch(params string[] ids)
    {
        _builder.ConflictsPatch(ids);
        return this;
    }

    public PrefabPatchLuaBuilder BeforePatch(params string[] ids)
    {
        _builder.BeforePatch(ids);
        return this;
    }

    public PrefabPatchLuaBuilder AfterPatch(params string[] ids)
    {
        _builder.AfterPatch(ids);
        return this;
    }

    public PrefabPatchLuaBuilder Before(params string[] modIds)
    {
        _builder.BeforeMod(modIds);
        return this;
    }

    public PrefabPatchLuaBuilder After(params string[] modIds)
    {
        _builder.AfterMod(modIds);
        return this;
    }

    public PrefabPatchLuaBuilder Configuration(params string[] inputs)
    {
        _builder.Configuration(inputs);
        return this;
    }

    public PrefabPatchLuaBuilder Set(
        string operationId,
        DynValue target,
        string propertyPath,
        DynValue value
    )
    {
        _builder.SetValue(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "operation target"),
            propertyPath,
            Value(value)
        );
        return this;
    }

    public PrefabPatchLuaBuilder Reference(
        string operationId,
        DynValue target,
        string propertyPath,
        DynValue reference
    )
    {
        _builder.SetObjectReference(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "operation target"),
            propertyPath,
            Model<PrefabPatchObjectReference>(
                reference,
                "object reference"
            )
        );
        return this;
    }

    public PrefabPatchLuaBuilder Active(
        string operationId,
        DynValue target,
        bool active
    )
    {
        _builder.SetActive(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "operation target"),
            active
        );
        return this;
    }

    public PrefabPatchLuaBuilder Suppress(
        string operationId,
        DynValue target
    )
    {
        _builder.SuppressObject(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "operation target")
        );
        return this;
    }

    public PrefabPatchLuaBuilder AddObject(
        string operationId,
        DynValue parent,
        DynValue fragment
    )
    {
        _builder.AddObject(
            operationId,
            parent.IsNil()
                ? null
                : Model<PrefabPatchObjectTarget>(
                    parent,
                    "parent target"
                ),
            Model<PrefabPatchObjectFragment>(
                fragment,
                "object fragment"
            )
        );
        return this;
    }

    public PrefabPatchLuaBuilder AddComponent(
        string operationId,
        DynValue target,
        DynValue component
    )
    {
        _builder.AddComponent(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "operation target"),
            Model<PrefabPatchComponentFragment>(
                component,
                "component fragment"
            )
        );
        return this;
    }

    public PrefabPatchLuaBuilder RemoveComponent(
        string operationId,
        DynValue target
    )
    {
        _builder.RemoveComponent(
            operationId,
            Model<PrefabPatchObjectTarget>(target, "component target")
        );
        return this;
    }

    public PrefabPatchManifest Build() => _builder.Build();

    public PrefabPatchManifest Register() => _builder.Register();

    private static PrefabPatchValue Value(DynValue value)
    {
        switch (value.Type)
        {
            case DataType.Boolean:
                return PrefabPatchValue.FromBoolean(value.Boolean);
            case DataType.Number:
                return PrefabPatchValue.FromFloat(value.Number);
            case DataType.String:
                return PrefabPatchValue.FromString(value.String);
            case DataType.Table:
            case DataType.UserData:
                return Model<PrefabPatchValue>(
                    value,
                    "typed prefab-patch value"
                );
            default:
                throw new ScriptRuntimeException(
                    $"Prefab patch values cannot be '{value.Type}'."
                );
        }
    }

    internal static T Model<T>(DynValue value, string description)
    {
        try
        {
            var token = JsonUserData.GetJTokenForDynValue(value);
            token = NormalizeEmptyTables(token, typeof(T));
            var serializer = JsonSerializer.Create(
                PrefabPatchJson.Settings
            );
            var result = token.ToObject<T>(serializer);
            if (result == null)
            {
                throw new ScriptRuntimeException(
                    $"Expected {description}, got nil."
                );
            }
            return result;
        }
        catch (ScriptRuntimeException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptRuntimeException(
                $"Invalid {description}: {exception.Message}"
            );
        }
    }

    private static JToken NormalizeEmptyTables(
        JToken token,
        Type expectedType
    )
    {
        if (
            token is JObject emptyObject
            && !emptyObject.Properties().Any()
            && IsCollection(expectedType)
        )
            return new JArray();
        if (token is JArray array)
        {
            var itemType = CollectionItemType(expectedType);
            if (itemType != null)
            {
                for (var index = 0; index < array.Count; index++)
                    array[index] = NormalizeEmptyTables(
                        array[index],
                        itemType
                    );
            }
            return array;
        }
        if (token is not JObject value)
            return token;

        const BindingFlags flags =
            BindingFlags.Instance | BindingFlags.Public;
        foreach (var field in expectedType.GetFields(flags))
        {
            var camelName =
                char.ToLowerInvariant(field.Name[0])
                + field.Name.Substring(1);
            var property = value.Property(
                    camelName,
                    StringComparison.OrdinalIgnoreCase
                )
                ?? value.Property(
                    field.Name,
                    StringComparison.OrdinalIgnoreCase
                );
            if (property != null)
                property.Value = NormalizeEmptyTables(
                    property.Value,
                    field.FieldType
                );
        }
        return value;
    }

    private static bool IsCollection(Type type) =>
        type.IsArray
        || (
            type != typeof(string)
            && typeof(IEnumerable).IsAssignableFrom(type)
        );

    private static Type CollectionItemType(Type type) =>
        type.IsArray
            ? type.GetElementType()
            : type.IsGenericType
                ? type.GetGenericArguments()[0]
                : null;
}
