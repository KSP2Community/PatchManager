using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PatchManager.CampaignPacks
{
    /// <summary>
    /// In-memory runtime catalog for loaded campaign pack definitions and resolved previews.
    /// </summary>
    public sealed class CampaignPackRuntimeCatalog
    {
        private readonly Dictionary<string, CampaignPackDefinition> _packs = new(StringComparer.Ordinal);
        private readonly Dictionary<string, TechTreeSetDefinition> _techTreeSets = new(StringComparer.Ordinal);
        private readonly Dictionary<string, MissionSetDefinition> _missionSets = new(StringComparer.Ordinal);
        private readonly Dictionary<string, ScienceSetDefinition> _scienceSets = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CampaignPackExtensionDefinition> _extensions = new(StringComparer.Ordinal);
        private readonly List<string> _issues = new();

        /// <summary>
        /// Loaded campaign pack definitions keyed by campaign pack id.
        /// </summary>
        public IReadOnlyDictionary<string, CampaignPackDefinition> Packs => _packs;

        /// <summary>
        /// Loaded tech tree set definitions keyed by set id.
        /// </summary>
        public IReadOnlyDictionary<string, TechTreeSetDefinition> TechTreeSets => _techTreeSets;

        /// <summary>
        /// Loaded mission set definitions keyed by set id.
        /// </summary>
        public IReadOnlyDictionary<string, MissionSetDefinition> MissionSets => _missionSets;

        /// <summary>
        /// Loaded science set definitions keyed by set id.
        /// </summary>
        public IReadOnlyDictionary<string, ScienceSetDefinition> ScienceSets => _scienceSets;

        /// <summary>
        /// Loaded campaign pack extension definitions keyed by extension id.
        /// </summary>
        public IReadOnlyDictionary<string, CampaignPackExtensionDefinition> Extensions => _extensions;

        /// <summary>
        /// Load and validation issues discovered while building the catalog.
        /// </summary>
        public IReadOnlyList<string> Issues => _issues;

        /// <summary>
        /// Adds a load-time issue to the catalog diagnostics.
        /// </summary>
        /// <param name="issue">Human-readable issue text.</param>
        public void AddLoadIssue(string issue)
        {
            _issues.Add($"[Load] {issue}");
        }

        /// <summary>
        /// Removes all loaded definitions and validation issues.
        /// </summary>
        public void Clear()
        {
            _packs.Clear();
            _techTreeSets.Clear();
            _missionSets.Clear();
            _scienceSets.Clear();
            _extensions.Clear();
            _issues.Clear();
        }

        /// <summary>
        /// Adds a campaign pack definition to the catalog.
        /// </summary>
        /// <param name="definition">Campaign pack definition to add.</param>
        /// <param name="sourceName">Optional source asset name used for diagnostics.</param>
        public void AddPack(CampaignPackDefinition definition, string sourceName = "")
        {
            AddDefinition(_packs, definition?.Id, definition, "Campaign pack", sourceName);
        }

        /// <summary>
        /// Adds a tech tree set definition to the catalog.
        /// </summary>
        /// <param name="definition">Tech tree set definition to add.</param>
        /// <param name="sourceName">Optional source asset name used for diagnostics.</param>
        public void AddTechTreeSet(TechTreeSetDefinition definition, string sourceName = "")
        {
            AddDefinition(_techTreeSets, definition?.Id, definition, "Tech tree set", sourceName);
        }

        /// <summary>
        /// Adds a mission set definition to the catalog.
        /// </summary>
        /// <param name="definition">Mission set definition to add.</param>
        /// <param name="sourceName">Optional source asset name used for diagnostics.</param>
        public void AddMissionSet(MissionSetDefinition definition, string sourceName = "")
        {
            AddDefinition(_missionSets, definition?.Id, definition, "Mission set", sourceName);
        }

        /// <summary>
        /// Adds a science set definition to the catalog.
        /// </summary>
        /// <param name="definition">Science set definition to add.</param>
        /// <param name="sourceName">Optional source asset name used for diagnostics.</param>
        public void AddScienceSet(ScienceSetDefinition definition, string sourceName = "")
        {
            AddDefinition(_scienceSets, definition?.Id, definition, "Science set", sourceName);
        }

        /// <summary>
        /// Adds a campaign pack extension definition to the catalog.
        /// </summary>
        /// <param name="definition">Campaign pack extension definition to add.</param>
        /// <param name="sourceName">Optional source asset name used for diagnostics.</param>
        public void AddExtension(CampaignPackExtensionDefinition definition, string sourceName = "")
        {
            AddDefinition(_extensions, definition?.Id, definition, "Campaign pack extension", sourceName);
        }

        /// <summary>
        /// Resolves the effective contents for a loaded campaign pack.
        /// </summary>
        /// <param name="packId">Campaign pack identifier.</param>
        /// <returns>Resolved contents, or <c>null</c> when the pack is unknown.</returns>
        public EffectiveCampaignPack Resolve(string packId)
        {
            if (string.IsNullOrWhiteSpace(packId) || !_packs.TryGetValue(packId, out var pack))
            {
                return null;
            }

            var result = new EffectiveCampaignPack
            {
                CampaignPackId = pack.Id,
                NameLocKey = pack.NameLocKey,
                DescriptionLocKey = pack.DescriptionLocKey,
                GalaxyDefinitionKey = pack.GalaxyDefinitionKey
            };

            if (!string.IsNullOrWhiteSpace(pack.TechTreeSetId) && _techTreeSets.TryGetValue(pack.TechTreeSetId, out var techTreeSet))
            {
                result.TechNodeIds.AddRange(CopyDistinct(techTreeSet.TechNodeIds));
            }

            if (!string.IsNullOrWhiteSpace(pack.MissionSetId) && _missionSets.TryGetValue(pack.MissionSetId, out var missionSet))
            {
                result.MissionIds.AddRange(CopyDistinct(missionSet.MissionIds));
            }

            if (!string.IsNullOrWhiteSpace(pack.ScienceSetId) && _scienceSets.TryGetValue(pack.ScienceSetId, out var scienceSet))
            {
                result.ExperimentIds.AddRange(CopyDistinct(scienceSet.ExperimentIds));
                result.ScienceRegionIds.AddRange(CopyDistinct(scienceSet.ScienceRegionIds));
                result.DiscoverableIds.AddRange(CopyDistinct(scienceSet.DiscoverableIds));
            }

            var matching = GetMatchingExtensions(pack).OrderBy(e => e.Id, StringComparer.Ordinal).ToList();
            foreach (var extension in matching)
            {
                AddRangeUnique(result.TechNodeIds, extension.AddTechNodeIds);
                AddRangeUnique(result.MissionIds, extension.AddMissionIds);
                AddRangeUnique(result.ExperimentIds, extension.AddExperimentIds);
                AddRangeUnique(result.ScienceRegionIds, extension.AddScienceRegionIds);
                AddRangeUnique(result.DiscoverableIds, extension.AddDiscoverableIds);
                if (!string.IsNullOrWhiteSpace(extension.Id))
                {
                    result.AppliedExtensionIds.Add(extension.Id);
                }
            }

            foreach (var extension in matching)
            {
                RemoveAll(result.TechNodeIds, extension.RemoveTechNodeIds);
                RemoveAll(result.MissionIds, extension.RemoveMissionIds);
                RemoveAll(result.ExperimentIds, extension.RemoveExperimentIds);
                RemoveAll(result.ScienceRegionIds, extension.RemoveScienceRegionIds);
                RemoveAll(result.DiscoverableIds, extension.RemoveDiscoverableIds);
            }

            return result;
        }

        /// <summary>
        /// Rebuilds runtime validation issues for all loaded definitions.
        /// </summary>
        public void Validate()
        {
            _issues.RemoveAll(issue => issue.StartsWith("[Validation]", StringComparison.Ordinal));

            foreach (var pack in _packs.Values)
            {
                if (string.IsNullOrWhiteSpace(pack.GalaxyDefinitionKey))
                {
                    AddValidationIssue($"Campaign pack '{pack.Id}' has no galaxy definition key.");
                }

                AddMissingReference(pack.Id, "tech tree set", pack.TechTreeSetId, _techTreeSets);
                AddMissingReference(pack.Id, "mission set", pack.MissionSetId, _missionSets);
                AddMissingReference(pack.Id, "science set", pack.ScienceSetId, _scienceSets);
            }

            foreach (var extension in _extensions.Values)
            {
                var hasTarget = !string.IsNullOrWhiteSpace(extension.TargetCampaignPackId) ||
                    !string.IsNullOrWhiteSpace(extension.TargetTechTreeSetId) ||
                    !string.IsNullOrWhiteSpace(extension.TargetMissionSetId) ||
                    !string.IsNullOrWhiteSpace(extension.TargetScienceSetId);

                if (!hasTarget)
                {
                    AddValidationIssue($"Campaign pack extension '{extension.Id}' does not target a pack or set.");
                }
                else if (!_packs.Values.Any(pack => TargetsPack(extension, pack)))
                {
                    AddValidationIssue($"Campaign pack extension '{extension.Id}' does not match any loaded campaign pack.");
                }

                AddConflictIssue(extension.Id, "tech node", extension.AddTechNodeIds, extension.RemoveTechNodeIds);
                AddConflictIssue(extension.Id, "mission", extension.AddMissionIds, extension.RemoveMissionIds);
                AddConflictIssue(extension.Id, "science experiment", extension.AddExperimentIds, extension.RemoveExperimentIds);
                AddConflictIssue(extension.Id, "science region", extension.AddScienceRegionIds, extension.RemoveScienceRegionIds);
                AddConflictIssue(extension.Id, "discoverable", extension.AddDiscoverableIds, extension.RemoveDiscoverableIds);
            }
        }

        /// <summary>
        /// Resolves all loaded campaign packs in stable id order.
        /// </summary>
        /// <returns>Resolved campaign pack previews.</returns>
        public IReadOnlyList<EffectiveCampaignPack> ResolveAll()
        {
            return _packs.Keys
                .OrderBy(id => id, StringComparer.Ordinal)
                .Select(Resolve)
                .OfType<EffectiveCampaignPack>()
                .ToList();
        }

        /// <summary>
        /// Builds a human-readable summary of loaded definitions, resolved contents, and diagnostics.
        /// </summary>
        /// <returns>Summary text suitable for logs and Patch Manager detail UI.</returns>
        public string BuildSummaryText()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Campaign Packs:");
            sb.AppendLine($"Packs: {_packs.Count}");
            sb.AppendLine($"Tech tree sets: {_techTreeSets.Count}");
            sb.AppendLine($"Mission sets: {_missionSets.Count}");
            sb.AppendLine($"Science sets: {_scienceSets.Count}");
            sb.AppendLine($"Extensions: {_extensions.Count}");

            var effectivePacks = ResolveAll();
            foreach (var pack in effectivePacks)
            {
                sb.AppendLine("");
                sb.AppendLine($"- {pack.CampaignPackId}");
                sb.AppendLine($"  Galaxy: {ValueOrNone(pack.GalaxyDefinitionKey)}");
                AppendIdList(sb, "Tech nodes", pack.TechNodeIds);
                AppendIdList(sb, "Missions", pack.MissionIds);
                AppendIdList(sb, "Experiments", pack.ExperimentIds);
                AppendIdList(sb, "Science regions", pack.ScienceRegionIds);
                AppendIdList(sb, "Discoverables", pack.DiscoverableIds);
                AppendIdList(sb, "Extensions", pack.AppliedExtensionIds);
            }

            if (_issues.Count > 0)
            {
                sb.AppendLine("");
                sb.AppendLine("Issues:");
                foreach (var issue in _issues)
                {
                    sb.AppendLine($"- {issue}");
                }
            }

            return sb.ToString();
        }

        private void AddDefinition<T>(Dictionary<string, T> target, string id, T definition, string kind, string sourceName)
            where T : class
        {
            if (definition == null)
            {
                AddLoadIssue($"{kind} from '{ValueOrNone(sourceName)}' could not be read.");
                return;
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                AddLoadIssue($"{kind} from '{ValueOrNone(sourceName)}' has no id.");
                return;
            }

            if (target.ContainsKey(id))
            {
                AddLoadIssue($"{kind} id '{id}' is duplicated; keeping the first loaded definition.");
                return;
            }

            target[id] = definition;
        }

        private IEnumerable<CampaignPackExtensionDefinition> GetMatchingExtensions(CampaignPackDefinition pack)
        {
            return _extensions.Values.Where(extension => TargetsPack(extension, pack));
        }

        private static bool TargetsPack(CampaignPackExtensionDefinition extension, CampaignPackDefinition pack)
        {
            return Matches(extension.TargetCampaignPackId, pack.Id) ||
                Matches(extension.TargetTechTreeSetId, pack.TechTreeSetId) ||
                Matches(extension.TargetMissionSetId, pack.MissionSetId) ||
                Matches(extension.TargetScienceSetId, pack.ScienceSetId);
        }

        private static bool Matches(string target, string actual)
        {
            return !string.IsNullOrWhiteSpace(target) &&
                !string.IsNullOrWhiteSpace(actual) &&
                string.Equals(target, actual, StringComparison.Ordinal);
        }

        private void AddMissingReference<T>(string packId, string kind, string id, Dictionary<string, T> known)
        {
            if (!string.IsNullOrWhiteSpace(id) && !known.ContainsKey(id))
            {
                AddValidationIssue($"Campaign pack '{packId}' references missing {kind} '{id}'.");
            }
        }

        private void AddConflictIssue(string extensionId, string kind, IEnumerable<string> additions, IEnumerable<string> removals)
        {
            var addSet = new HashSet<string>((additions ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)));
            foreach (var id in (removals ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct())
            {
                if (addSet.Contains(id))
                {
                    AddValidationIssue($"Campaign pack extension '{extensionId}' both adds and removes {kind} '{id}'. Removal will win.");
                }
            }
        }

        private void AddValidationIssue(string issue)
        {
            _issues.Add($"[Validation] {issue}");
        }

        private static IEnumerable<string> CopyDistinct(IEnumerable<string> values)
        {
            return (values ?? Enumerable.Empty<string>()).Where(value => !string.IsNullOrWhiteSpace(value)).Distinct();
        }

        private static void AddRangeUnique(List<string> target, IEnumerable<string> values)
        {
            if (values == null) return;
            foreach (var value in values)
            {
                if (string.IsNullOrWhiteSpace(value) || target.Contains(value)) continue;
                target.Add(value);
            }
        }

        private static void RemoveAll(List<string> target, IEnumerable<string> values)
        {
            if (values == null) return;
            var removals = new HashSet<string>(values.Where(value => !string.IsNullOrWhiteSpace(value)));
            target.RemoveAll(removals.Contains);
        }

        private static string ValueOrNone(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "<none>" : value;
        }

        private static void AppendIdList(StringBuilder sb, string label, IReadOnlyCollection<string> ids)
        {
            sb.AppendLine($"  {label} ({ids.Count}):");

            if (ids.Count == 0)
            {
                sb.AppendLine("    <none>");
                return;
            }

            foreach (var id in ids)
            {
                sb.AppendLine($"    - {id}");
            }
        }
    }
}
