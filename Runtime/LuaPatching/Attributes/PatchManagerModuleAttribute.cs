using System;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MoonSharp.Interpreter;

namespace PatchManager.LuaPatching.Attributes;

/// <summary>
/// Marks a class as a PatchManager submodule that Lua scripts access via <c>PM.&lt;SubmoduleName&gt;</c>.
/// </summary>
/// <remarks>
/// The decorated type must also carry <see cref="MoonSharpUserDataAttribute" /> -- modules without it are skipped
/// at universe-init time. The class must expose a constructor taking <see cref="Builtin.PatchManagerCore" />
/// followed by <see cref="Universe" />; the universe instantiates one instance and exposes it under the configured
/// name on the global <c>PM</c> table.
/// </remarks>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public class PatchManagerModuleAttribute : Attribute
{
	/// <summary>
	/// The name the submodule is exposed under on <c>PM</c>.
	/// </summary>
	public string SubmoduleName;

	/// <summary>
	/// Creates the attribute with the given submodule name.
	/// </summary>
	/// <param name="submoduleName">The name the submodule is exposed under on <c>PM</c>.</param>
	public PatchManagerModuleAttribute(string submoduleName)
	{
		SubmoduleName = submoduleName;
	}
}
