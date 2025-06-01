using System;
using KSP.Game;
using KSP.Messages;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using PatchManager.Shared;
using UnityEngine;

namespace PatchManager.Resources
{
    /// <summary>
    /// Handles dynamic resizing of non-stageable resources flight HUD window based on number of resources
    /// </summary>
    public class NonStageableResourcesUIController : KerbalMonoBehaviour
    {
        private const string NON_STAGEABLE_RESOURCES_UI_PANEL_PATH = "GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Scaled Main Canvas/FlightHudRoot(Clone)/NonStageableResources(Clone)/KSP2UIWindow/Root/UIPanel";
        private VesselComponent? _activeVessel;
        private GameObject? _nsResourcesUI;
        private bool _needNsUiUpdate = false;

        private void Start()
        {
            Game.Messages.PersistentSubscribe<VesselChangedMessage>(OnVesselChanged);
        }

        private void OnDestroy()
        {
            if (IsGameShuttingDown)
                return;
            Game.Messages.Unsubscribe<VesselChangedMessage>(OnVesselChanged);
        }

        private void Update()
        {
            // Get active vessel
            _activeVessel = Game?.ViewController?.GetActiveSimVessel();

            if (_activeVessel is null)
                return;

            if (!_needNsUiUpdate) return;
            
            _needNsUiUpdate = false;

            // Update Non Stageable Resources UI size
            UpdateNonStageableResourcesUI();
        }

        private void OnVesselChanged(MessageCenterMessage msg)
        {
            if (msg is not VesselChangedMessage vesselChangedMessage || vesselChangedMessage == null)
                return;

            _needNsUiUpdate = true;
        }

        /// <summary>
        /// Updates the height & position of the non-stageable resources UI
        /// </summary>
        /// <param name="nsResourcesCount"></param>
        private void UpdateNonStageableResourcesUI()
        {
            // Get number of non-stageable resources in the active vessel
            int nsResourcesCount = GetNonStageableResourcesCount(_activeVessel);

            Logging.LogInfo(
                $"Updating the Non-Stageable Resource UI. Found {nsResourcesCount} Resources to display.");

            // Find the non-stageable resources window game object
            _nsResourcesUI =
                GameObject.Find(
                    NON_STAGEABLE_RESOURCES_UI_PANEL_PATH);

            if (_nsResourcesUI == null) return;
            
            // Number of resources above the default maximum of 2 (EC & Monoprop)
            var extraResources = Math.Max(0, nsResourcesCount - 2);

            // Get the UI object's RectTransform
            var rect = _nsResourcesUI.GetComponent<RectTransform>();

            // Increase the UI object's vertical size
            rect.sizeDelta = new Vector2(0, 30 * extraResources);

            // Shift the UI object down
            rect.localPosition = extraResources * 15 * Vector3.down;
        }

        /// <summary>
        /// Returns the number of non-stageable resources present on the vessel
        /// </summary>
        private int GetNonStageableResourcesCount(VesselComponent vessel)
        {
            var count = 0;

            var controlOwner = vessel.GetControlOwner();
            
            if (controlOwner == null || controlOwner.PartOwner == null) return count;
            
            var containerGroup = controlOwner.PartOwner.ContainerGroup;
            
            foreach (var containedResourceData in containerGroup.GetAllResourcesContainedData())
            {
                var definitionData =
                    Game.ResourceDefinitionDatabase.GetDefinitionData(containedResourceData.ResourceID);
                if (definitionData.resourceProperties.NonStageable)
                    ++count;
            }

            return count;
        }
    }
}