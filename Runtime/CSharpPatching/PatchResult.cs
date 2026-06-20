namespace PatchManager.CSharpPatching
{
    /// <summary>
    /// The outcome a C# patch method returns.
    /// </summary>
    /// <remarks>
    /// Mirrors the Lua patch convention of returning "remove" or nil.
    /// </remarks>
    public enum PatchResult
    {
        /// <summary>
        /// Keep the asset. The default for void patch methods.
        /// </summary>
        Keep,

        /// <summary>
        /// Delete the asset, equivalent to a Lua patch returning "remove".
        /// </summary>
        Remove
    }
}
