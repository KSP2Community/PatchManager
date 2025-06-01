using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace PatchManager.Editor
{
    [ScriptedImporter(1, "patch")]
    public class PatchImporter: ScriptedImporter
    {
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var asset = new TextAsset(File.ReadAllText(ctx.assetPath));
            ctx.AddObjectToAsset(ctx.assetPath, asset);
            ctx.SetMainObject(asset);
        }
    }
}