using System;
using System.Reflection;
using HarmonyLib;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;
using UnityEngine;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// Wraps CLR exceptions that escape MoonSharp's <c>Processor.Internal_ExecCall</c> in a
    /// <see cref="ScriptRuntimeException" /> with the Lua source location and the original CLR stack trace.
    /// </summary>
    /// <remarks>
    /// Upstream does <c>catch (Exception e) { throw e; }</c>, which both strips the stack and bypasses MoonSharp's
    /// interpreter-exception channel, so consumers see a bare CLR exception with no script context. The Harmony
    /// finalizer this class installs swaps the escaping exception for a <see cref="ScriptRuntimeException" /> whose
    /// message embeds the original type, message, and stack alongside MoonSharp's standard source-line decoration.
    /// </remarks>
    internal static class MoonSharpExceptionWrapPatch
    {
        private static bool _installed;

        private static readonly Type ProcessorType =
            typeof(Script).Assembly.GetType("MoonSharp.Interpreter.Execution.VM.Processor");

        private static readonly FieldInfo ScriptField = ProcessorType?.GetField(
            "m_Script", BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo GetCurrentSourceRefMethod = ProcessorType?.GetMethod(
            "GetCurrentSourceRef",
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, new[] { typeof(int) }, null);

        private static readonly MethodInfo DecorateMessageMethod = typeof(InterpreterException).GetMethod(
            "DecorateMessage", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Installs the Harmony finalizer once. Subsequent calls are no-ops.
        /// </summary>
        /// <remarks>
        /// Logs a warning and returns silently when MoonSharp's internal types or methods cannot be resolved, since
        /// the patch is a diagnostic aid and its absence should not block patching.
        /// </remarks>
        public static void Install()
        {
            if (_installed) return;
            try
            {
                if (ProcessorType == null)
                {
                    Debug.LogWarning("[PatchManager] MoonSharp Processor type not found; exception-wrap patch not installed.");
                    return;
                }

                var target = AccessTools.Method(ProcessorType, "Internal_ExecCall");
                if (target == null)
                {
                    Debug.LogWarning("[PatchManager] MoonSharp Processor.Internal_ExecCall not found; exception-wrap patch not installed.");
                    return;
                }

                var finalizer = new HarmonyMethod(typeof(MoonSharpExceptionWrapPatch).GetMethod(
                    nameof(WrapEscapingException), BindingFlags.NonPublic | BindingFlags.Static));

                new Harmony("ksp2redux.patchmanager.moonsharp.exceptionwrap").Patch(target, finalizer: finalizer);
                _installed = true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[PatchManager] Failed to install MoonSharp exception-wrap patch: {e}");
            }
        }

        // Harmony finalizer: returning a non-null Exception replaces what the original method threw.
        private static Exception WrapEscapingException(object __instance, int instructionPtr, Exception __exception)
        {
            if (__exception == null || __exception is InterpreterException) return __exception;

            try
            {
                var script = ScriptField?.GetValue(__instance) as Script;
                var sref = GetCurrentSourceRefMethod?.Invoke(__instance, new object[] { instructionPtr - 1 }) as SourceRef;

                var wrapped = new ScriptRuntimeException(
                    $"{__exception.GetType().FullName}: {__exception.Message}\n--- CLR stack ---\n{__exception.StackTrace}");

                if (script != null && sref != null && DecorateMessageMethod != null)
                {
                    DecorateMessageMethod.Invoke(wrapped, new object[] { script, sref, instructionPtr });
                }

                return wrapped;
            }
            catch
            {
                return __exception;
            }
        }
    }
}
