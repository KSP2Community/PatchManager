using System;
using KSP.Game;
using Newtonsoft.Json;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace PatchManager.CampaignPacks
{
    /// <summary>
    /// Loads baked campaign pack JSON and exposes a read-only runtime inspection summary.
    /// </summary>
    public sealed class CampaignPacksModule : BaseModule
    {
        /// <summary>
        /// Addressables label used for baked campaign pack JSON assets.
        /// </summary>
        public const string CampaignPacksLabel = "campaign_packs";

        /// <summary>
        /// Addressables label used for baked campaign pack tech tree set JSON assets.
        /// </summary>
        public const string TechTreeSetsLabel = "campaign_pack_tech_tree_sets";

        /// <summary>
        /// Addressables label used for baked campaign pack mission set JSON assets.
        /// </summary>
        public const string MissionSetsLabel = "campaign_pack_mission_sets";

        /// <summary>
        /// Addressables label used for baked campaign pack science set JSON assets.
        /// </summary>
        public const string ScienceSetsLabel = "campaign_pack_science_sets";

        /// <summary>
        /// Addressables label used for baked campaign pack extension JSON assets.
        /// </summary>
        public const string ExtensionsLabel = "campaign_pack_extensions";

        /// <summary>
        /// Runtime catalog populated from baked campaign pack JSON assets.
        /// </summary>
        public static CampaignPackRuntimeCatalog Catalog { get; } = new();

        private int _pendingLoads;
        private bool _loadStarted;
        private TextElement _detailsText;

        /// <inheritdoc />
        public override void Load()
        {
            Catalog.Clear();
            _pendingLoads = 5;
            _loadStarted = true;

            LoadDefinitions<CampaignPackDefinition>(
                CampaignPacksLabel,
                (definition, sourceName) => Catalog.AddPack(definition, sourceName));
            LoadDefinitions<TechTreeSetDefinition>(
                TechTreeSetsLabel,
                (definition, sourceName) => Catalog.AddTechTreeSet(definition, sourceName));
            LoadDefinitions<MissionSetDefinition>(
                MissionSetsLabel,
                (definition, sourceName) => Catalog.AddMissionSet(definition, sourceName));
            LoadDefinitions<ScienceSetDefinition>(
                ScienceSetsLabel,
                (definition, sourceName) => Catalog.AddScienceSet(definition, sourceName));
            LoadDefinitions<CampaignPackExtensionDefinition>(
                ExtensionsLabel,
                (definition, sourceName) => Catalog.AddExtension(definition, sourceName));
        }

        /// <inheritdoc />
        public override VisualElement GetDetails()
        {
            var foldout = new Foldout
            {
                text = "PatchManager.CampaignPacks",
                visible = true,
                style =
                {
                    display = DisplayStyle.Flex
                }
            };

            _detailsText = new TextElement
            {
                visible = true,
                style =
                {
                    display = DisplayStyle.Flex,
                    whiteSpace = WhiteSpace.Normal
                }
            };
            RefreshDetailsText();
            foldout.Add(_detailsText);
            return foldout;
        }

        private void LoadDefinitions<T>(string label, Action<T, string> addDefinition)
            where T : class
        {
            var handle = GameManager.Instance.Assets.LoadAssetsAsync<TextAsset>(
                label,
                asset => ImportDefinition(asset, addDefinition));

            handle.Completed += result =>
            {
                if (result.Status != AsyncOperationStatus.Succeeded)
                {
                    Logging.LogWarning($"[Campaign Packs] Failed to load addressable label '{label}'.");
                    Catalog.AddLoadIssue($"Failed to load addressable label '{label}'.");
                }

                Addressables.Release(handle);
                OnLabelLoadFinished();
            };
        }

        private static void ImportDefinition<T>(TextAsset asset, Action<T, string> addDefinition)
            where T : class
        {
            if (!asset)
            {
                return;
            }

            try
            {
                var definition = JsonConvert.DeserializeObject<T>(asset.text);
                addDefinition(definition, asset.name);
            }
            catch (Exception ex)
            {
                Logging.LogWarning($"[Campaign Packs] Failed to parse '{asset.name}': {ex.Message}");
                Catalog.AddLoadIssue($"Failed to parse '{asset.name}': {ex.Message}");
            }
        }

        private void OnLabelLoadFinished()
        {
            _pendingLoads--;
            if (_pendingLoads > 0)
            {
                RefreshDetailsText();
                return;
            }

            Catalog.Validate();
            LogSummary();
            RefreshDetailsText();
        }

        private static void LogSummary()
        {
            Logging.LogInfo($"[Campaign Packs]\n{Catalog.BuildSummaryText()}");
            foreach (var issue in Catalog.Issues)
            {
                Logging.LogWarning($"[Campaign Packs] {issue}");
            }
        }

        private void RefreshDetailsText()
        {
            if (_detailsText == null)
            {
                return;
            }

            var loading = _loadStarted && _pendingLoads > 0
                ? $"Loading campaign pack labels... {_pendingLoads} remaining.\n\n"
                : string.Empty;
            _detailsText.text = loading + Catalog.BuildSummaryText();
        }
    }
}
