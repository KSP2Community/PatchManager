using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace PatchManager.PrefabPatching;

/// <summary>
/// Atomic, recoverable, per-prefab resolved-plan cache. A cache hit avoids
/// dependency ordering, target validation, and conflict analysis.
/// </summary>
public sealed class PrefabPatchPlanCache
{
    public sealed class Result
    {
        public PrefabPatchResolvedPlan Plan;
        public bool CacheHit;
        public bool RecoveredBackup;
        public long ElapsedMilliseconds;
        public string Path;
    }

    private readonly string _directory;

    public PrefabPatchPlanCache(string directory)
    {
        _directory = Path.GetFullPath(directory);
    }

    public Result LoadOrResolve(
        IEnumerable<PrefabPatchManifest> source,
        ISet<string> activeModIds,
        string unityVersion,
        string targetPlatform
    )
    {
        var stopwatch = Stopwatch.StartNew();
        var manifests = source
            .Where(value => value != null)
            .OrderBy(value => value.PatchId, StringComparer.Ordinal)
            .ToList();
        if (manifests.Count == 0)
        {
            var empty = PrefabPatchResolver.Resolve(
                manifests,
                activeModIds,
                unityVersion,
                targetPlatform
            );
            stopwatch.Stop();
            return new Result
            {
                Plan = empty,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
            };
        }

        Directory.CreateDirectory(_directory);
        var sourceFingerprint = CalculateSourceFingerprint(
            manifests,
            activeModIds,
            unityVersion,
            targetPlatform
        );
        var path = GetPath(manifests[0].TargetPrefab);
        if (TryRead(path, sourceFingerprint, out var cached))
        {
            stopwatch.Stop();
            return new Result
            {
                Plan = cached,
                CacheHit = true,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Path = path
            };
        }

        var backupPath = path + ".bak";
        if (TryRead(backupPath, sourceFingerprint, out cached))
        {
            AtomicWrite(path, PrefabPatchJson.Serialize(cached));
            stopwatch.Stop();
            return new Result
            {
                Plan = cached,
                CacheHit = true,
                RecoveredBackup = true,
                ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
                Path = path
            };
        }

        var plan = PrefabPatchResolver.Resolve(
            manifests,
            activeModIds,
            unityVersion,
            targetPlatform
        );
        plan.SourceFingerprint = sourceFingerprint;
        if (plan.IsValid)
            AtomicWrite(path, PrefabPatchJson.Serialize(plan));
        stopwatch.Stop();
        return new Result
        {
            Plan = plan,
            CacheHit = false,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds,
            Path = path
        };
    }

    public string GetPath(PrefabPatchPrefabIdentity target)
    {
        var identity = target?.CanonicalKey ?? target?.Address ?? "invalid";
        return Path.Combine(
            _directory,
            PrefabPatchJson.Sha256(identity) + ".plan.json"
        );
    }

    public static string CalculateSourceFingerprint(
        IEnumerable<PrefabPatchManifest> manifests,
        ISet<string> activeModIds,
        string unityVersion,
        string targetPlatform
    )
    {
        var ordered = manifests
            .Where(value => value != null)
            .OrderBy(value => value.PatchId, StringComparer.Ordinal)
            .ToList();
        foreach (var manifest in ordered)
        {
            foreach (var operation in manifest.Operations)
                operation.PatchId = manifest.PatchId;
            manifest.ManifestHash = PrefabPatchJson.CalculateManifestHash(manifest);
        }
        var value = new
        {
            UnityVersion = unityVersion,
            TargetPlatform = targetPlatform,
            SchemaVersion = PrefabPatchSchema.Version,
            ComposerVersion = PrefabPatchSchema.ComposerVersion,
            Target = ordered.FirstOrDefault()?.TargetPrefab,
            ActiveMods = activeModIds.OrderBy(
                id => id,
                StringComparer.Ordinal
            ),
            Manifests = ordered.Select(
                manifest => new
                {
                    manifest.PatchId,
                    manifest.ManifestHash,
                    manifest.ConfigurationInputs
                }
            )
        };
        return PrefabPatchJson.Sha256(PrefabPatchJson.Serialize(value));
    }

    private static bool TryRead(
        string path,
        string sourceFingerprint,
        out PrefabPatchResolvedPlan plan
    )
    {
        plan = null;
        if (!File.Exists(path))
            return false;
        try
        {
            plan = PrefabPatchJson.Deserialize<PrefabPatchResolvedPlan>(
                File.ReadAllText(path)
            );
            return plan != null
                && plan.SchemaVersion == PrefabPatchSchema.Version
                && plan.ComposerVersion == PrefabPatchSchema.ComposerVersion
                && plan.IsValid
                && string.Equals(
                    plan.SourceFingerprint,
                    sourceFingerprint,
                    StringComparison.Ordinal
                );
        }
        catch
        {
            plan = null;
            return false;
        }
    }

    private static void AtomicWrite(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);
        if (string.IsNullOrWhiteSpace(directory))
            throw new InvalidOperationException(
                $"Cache path '{path}' has no directory."
            );
        Directory.CreateDirectory(directory);
        var tempPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        var backupPath = path + ".bak";
        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(contents);
            using (
                var stream = new FileStream(
                    tempPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough
                )
            )
            {
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(true);
            }

            if (File.Exists(path))
            {
                try
                {
                    File.Replace(tempPath, path, backupPath, true);
                }
                catch (PlatformNotSupportedException)
                {
                    File.Copy(path, backupPath, true);
                    File.Delete(path);
                    File.Move(tempPath, path);
                }
            }
            else
            {
                File.Move(tempPath, path);
            }
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}
