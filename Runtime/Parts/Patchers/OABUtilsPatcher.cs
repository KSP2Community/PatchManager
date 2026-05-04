using System.Collections.Generic;
using KSP.OAB;

namespace PatchManager.Parts.Patchers
{
    /// <summary>
    /// Static state container holding the part-name -> icon-name map used by the OAB utils Harmony patches.
    /// </summary>
    internal class OabUtilsPatcher
    {
        /// <summary>
        /// Map of part name to icon-asset name. Populated externally and consulted by the OAB icon lookup.
        /// </summary>
        internal static Dictionary<string,string> PartIconMap { get; } = new();
    }
}
