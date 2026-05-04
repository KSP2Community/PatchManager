using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Attributes;

/// <summary>
/// Registers a patch manager lua module
/// Derivers must have [MoonSharpUserData] module attribute
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class PatchManagerModuleAttribute : Attribute
{
    public string SubmoduleName;
	public PatchManagerModuleAttribute(string submoduleName)
	{
		SubmoduleName = submoduleName;
	}
}