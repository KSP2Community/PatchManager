using JetBrains.Annotations;
using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;

namespace PatchManager.LuaPatching
{
    /// <summary>
    /// Basically represents a converter instance that can convert (based on a ruleset) to and from a JSON string
    /// </summary>
    public interface IConverter
    {
        /// <summary>
        /// Creates a dynamic value from json which is what will be passed to patch functions
        /// </summary>
        /// <param name="json">The JSON to be converted</param>
        /// <returns>A DynValue representing whatever object was given</returns>
        public DynValue FromJson(JToken json);

        /// <summary>
        /// Converts a dynvalue 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [CanBeNull]
        public JToken ToJson(DynValue value);
    }
}