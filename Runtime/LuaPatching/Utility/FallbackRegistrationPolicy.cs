using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
using PatchManager.LuaPatching;
namespace PatchManager.LuaPatching.Utility;

/// <summary>
/// MoonSharp <see cref="IRegistrationPolicy" /> that wraps every UserData type with a string indexer in an
/// <see cref="IndexerFallbackDescriptor" /> and disallows automatic type registration.
/// </summary>
/// <remarks>
/// Installed as the global <c>UserData.RegistrationPolicy</c> by <see cref="Universe" />'s static constructor,
/// so any <see cref="JsonUserData" /> subclass registered later automatically gets member-lookup-then-indexer
/// dispatch without having to opt in per-type. Auto-registration is disabled to keep the Lua-exposed type set
/// explicit; every type must carry <see cref="MoonSharpUserDataAttribute" /> to be visible from scripts.
/// </remarks>
public class FallbackRegistrationPolicy : IRegistrationPolicy
{
    /// <summary>
    /// Wraps <paramref name="newDescriptor" /> in an <see cref="IndexerFallbackDescriptor" /> when its type
    /// declares a string indexer; otherwise returns it unchanged.
    /// </summary>
    /// <param name="newDescriptor">The descriptor being registered, or <c>null</c> for a deregistration.</param>
    /// <param name="oldDescriptor">The previously registered descriptor for this type, when any.</param>
    /// <returns>The descriptor to register, or <c>null</c> to deregister.</returns>
    public IUserDataDescriptor HandleRegistration(IUserDataDescriptor newDescriptor, IUserDataDescriptor oldDescriptor)
    {
        // If newDescriptor is null, it's a deregistration request
        if (newDescriptor == null) return null;

        // Check if the type has a string-based indexer
        if (HasStringIndexer(newDescriptor.Type))
        {
            // Wrap the standard descriptor with our fallback logic
            return new IndexerFallbackDescriptor(newDescriptor);
        }

        return newDescriptor;
    }

    /// <summary>
    /// Disallows MoonSharp's on-the-fly type registration so the Lua-exposed type set stays explicit.
    /// </summary>
    /// <param name="type">The type MoonSharp would auto-register.</param>
    /// <returns>Always false.</returns>
    public bool AllowTypeAutoRegistration(Type type)
    {
        // Usually false for security/performance, but you can set to true
        // if you want MoonSharp to register types on the fly.
        return false;
    }

    private bool HasStringIndexer(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Any(p => p.GetIndexParameters()
                .Any(param => param.ParameterType == typeof(string)));
    }
}
