using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PatchManager.Core.Cache.Json
{
    /// <summary>
    /// Per-mod replay slice carrying identity (descriptor versus standalone, plus the .lua path for standalones)
    /// and the bindings the mod's patches declared. Stored as <see cref="Inventory.SerializedConfigs" />
    /// values so <see cref="ConfigReplay" /> can decide at replay time whether the mod still exists
    /// before re-binding its entries.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ConfigReplaySlice
    {
        /// <summary>
        /// True if the patch is a single standalone .lua file (no SpaceWarp descriptor), false for descriptor mods.
        /// </summary>
        [JsonProperty("is_standalone")]
        public bool IsStandalone { get; set; }

        /// <summary>
        /// For standalone patches, the absolute path to the .lua file. Null for descriptor mods.
        /// Compared against <see cref="System.IO.File.Exists(string)" /> at replay time to verify the
        /// patch is still on disk.
        /// </summary>
        [JsonProperty("lua_path", NullValueHandling = NullValueHandling.Ignore)]
        public string LuaPath { get; set; }

        /// <summary>
        /// The bindings this mod's patches declared, keyed by composite <c>section/name</c>.
        /// </summary>
        [JsonProperty("entries", Required = Required.Always)]
        public Dictionary<string, ConfigReplayEntry> Entries { get; set; } = new();
    }

    /// <summary>
    /// Persisted record of a single <c>Config:</c> binding declared by a Lua patch. Stored under
    /// <see cref="ConfigReplaySlice.Entries" /> and replayed by <see cref="ConfigReplay" /> so the
    /// binding survives hot-cache launches that skip running the Lua scripts.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ConfigReplayEntry
    {
        /// <summary>
        /// Type tag matching the <c>Config:</c> method that produced the entry: <c>bool</c>, <c>float</c>,
        /// <c>integer</c>, <c>string</c>, or <c>color</c>.
        /// </summary>
        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }

        /// <summary>
        /// The config-file section the entry lives in.
        /// </summary>
        [JsonProperty("section", Required = Required.Always)]
        public string Section { get; set; }

        /// <summary>
        /// The entry's key within its section.
        /// </summary>
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; set; }

        /// <summary>
        /// Description string the patch declared.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; } = "";

        /// <summary>
        /// Default value the patch declared, serialized as JSON.
        /// </summary>
        [JsonProperty("default")]
        public JToken Default { get; set; }

        /// <summary>
        /// The value the entry held the last time the patch ran. Compared against the current value at
        /// replay time to detect whether the cache should be invalidated.
        /// </summary>
        [JsonProperty("ran_with")]
        public JToken RanWith { get; set; }

        /// <summary>
        /// Optional constraint declared by the patch, in a serialized union shape.
        /// </summary>
        [JsonProperty("constraint", NullValueHandling = NullValueHandling.Ignore)]
        public ConfigReplayConstraint Constraint { get; set; }
    }

    /// <summary>
    /// Persisted form of an <see cref="ReduxLib.Configuration.IValueConstraint" /> for replay.
    /// Discriminated by <see cref="Kind" />: <c>range</c> uses <see cref="Min" /> / <see cref="Max" />,
    /// <c>list</c> uses <see cref="Values" />.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ConfigReplayConstraint
    {
        /// <summary>
        /// <c>range</c> for <see cref="ReduxLib.Configuration.RangeConstraint{T}" />, <c>list</c> for
        /// <see cref="ReduxLib.Configuration.ListConstraint{T}" />.
        /// </summary>
        [JsonProperty("kind", Required = Required.Always)]
        public string Kind { get; set; }

        /// <summary>
        /// Inclusive minimum for a <c>range</c> constraint. Unused for other kinds.
        /// </summary>
        [JsonProperty("min", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Min { get; set; }

        /// <summary>
        /// Inclusive maximum for a <c>range</c> constraint. Unused for other kinds.
        /// </summary>
        [JsonProperty("max", NullValueHandling = NullValueHandling.Ignore)]
        public JToken Max { get; set; }

        /// <summary>
        /// Acceptable values for a <c>list</c> constraint. Unused for other kinds.
        /// </summary>
        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public JArray Values { get; set; }
    }
}
