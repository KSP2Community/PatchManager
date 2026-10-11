using System;
using System.Collections.Generic;
using PatchManager.Core.Cache.Json;

namespace PatchManager.Core.Cache
{
    /// <summary>
    /// The patch cache on disk: archives and prefab plans shared by every feature set, and one inventory per set
    /// naming the archives and plans that set uses.
    /// </summary>
    /// <remarks>
    /// Archives are named by their contents, so two sets whose label patches out identical share one file.
    /// </remarks>
    internal class CacheStore : IDisposable
    {
        private Inventory _inventory;

        /// <summary>
        /// Opens the cache rooted at a directory, creating nothing until something is written.
        /// </summary>
        /// <param name="root">The cache's root directory.</param>
        public CacheStore(string root)
        {
            Root = root;
        }

        /// <summary>
        /// Gets the cache's root directory.
        /// </summary>
        public string Root { get; }

        /// <summary>
        /// Gets the directory holding the patched-asset archives every set shares.
        /// </summary>
        public string ArchivesDirectory => Root;

        /// <summary>
        /// Gets the directory holding the prefab patch plans every set shares.
        /// </summary>
        public string PlansDirectory => Root;

        /// <summary>
        /// Gets the id of the set whose inventory is open.
        /// </summary>
        public string ActiveSet { get; private set; }

        /// <summary>
        /// Gets the open set's inventory, loaded from disk on first access.
        /// </summary>
        public Inventory Inventory => _inventory ??= Inventory.Create();

        /// <summary>
        /// Opens a set's inventory and marks the set as used now.
        /// </summary>
        /// <param name="setId">The set's id, which names its directory.</param>
        public void UseSet(string setId)
        {
            ActiveSet = setId;
        }

        /// <summary>
        /// Deletes the open set's inventory and starts it afresh, leaving every other set and every archive alone.
        /// </summary>
        public void InvalidateSet()
        {
        }

        /// <summary>
        /// Writes the open set's inventory to disk.
        /// </summary>
        public void SaveInventory()
        {
        }

        /// <summary>
        /// Stores a label's patched assets, unless an archive with exactly these contents is already stored.
        /// </summary>
        /// <param name="entries">The archive's entries as name and text pairs.</param>
        /// <returns>The archive's filename, relative to <see cref="ArchivesDirectory" />.</returns>
        public string WriteArchive(IReadOnlyCollection<(string name, string text)> entries) => "";

        /// <summary>
        /// Opens a stored archive, or returns the copy already open.
        /// </summary>
        /// <param name="archiveFilename">The archive's filename, relative to <see cref="ArchivesDirectory" />.</param>
        /// <returns>The archive.</returns>
        public Archive GetArchive(string archiveFilename) => throw new NotImplementedException();

        /// <summary>
        /// Deletes the least recently used sets above a limit, then every archive and plan no remaining set uses.
        /// </summary>
        /// <remarks>
        /// The open set is never evicted.
        /// </remarks>
        /// <param name="maxSets">How many sets may stay.</param>
        /// <param name="pinned">The ids of sets kept whatever their age.</param>
        public void Evict(int maxSets, ISet<string> pinned)
        {
        }

        /// <summary>
        /// Closes every open archive.
        /// </summary>
        public void Dispose()
        {
        }
    }
}
