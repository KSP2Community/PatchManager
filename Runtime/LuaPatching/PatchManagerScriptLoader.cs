using System.IO;
using System.Linq;
using JetBrains.Annotations;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// Custom script loader for loading modules and files using patch manager (and regular mods later on)
    /// </summary>
    public class PatchManagerScriptLoader : IScriptLoader
    {
        public object LoadFile(string file, Table globalContext)
        {
            if (!File.Exists(file))
            {
                throw new ScriptRuntimeException($"File {file} does not exist.");
            }
            return File.ReadAllText(file);
        }

        [CanBeNull]
        private string GetRoot(string pluginGuid)
        {
            if (SpaceWarp2.API.Mods.PluginList.TryGetDescriptor(pluginGuid) is not {} plugin) 
                return null;
            return plugin.Folder.FullName;
        }


        /// <inheritdoc />
        public string ResolveFileName(string filename, Table globalContext)
        {
            return Path.Combine(globalContext.Get("Location").CastToString(),filename);
        }

        [CanBeNull]
        private static string FindInFolder(string folder, string modname)
        {
            var dirInfo = new DirectoryInfo(folder);
            var needle = $"_{modname}.lua";
            return dirInfo.EnumerateFiles(needle, SearchOption.AllDirectories).Select(file => file.FullName).FirstOrDefault();
        }
        
        /// <inheritdoc />
        public string ResolveModuleName(string modname, Table globalContext)
        {
            // So we are going to namespace modules by mod:filename
            // But we want to easily be able to access local libraries
            // Modules always have an _... convention

            if (modname.Contains(':'))
            {
                var split = modname.Split(':');
                var root = GetRoot(split[0]);
                return root == null ? null : FindInFolder(root, split[1]);
            }
            else
            {
                var root = GetRoot(globalContext.Get("ModId").CastToString());
                return root == null ? null : FindInFolder(root, modname);
            }
        }
    }
}