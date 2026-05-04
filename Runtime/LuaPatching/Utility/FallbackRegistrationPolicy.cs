using System;
using System.Linq;
using System.Reflection;
using MoonSharp.Interpreter.Interop;
using MoonSharp.Interpreter.Interop.RegistrationPolicies;
namespace PatchManager.Runtime.LuaPatching.Utility;

public class FallbackRegistrationPolicy : IRegistrationPolicy
{
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