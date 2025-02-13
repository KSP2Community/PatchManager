using System.Collections.Generic;
using KSP.OAB;

namespace PatchManager.Parts.Patchers
{
    internal class OabUtilsPatcher
    {
        /// <summary>
        /// This is a map of part names to icon names. It is populated by the PartDataDeserializePatcher.
        /// </summary>
        internal static Dictionary<string,string> PartIconMap { get; } = new();
    }
}