using KSP.Game;
using KSP.IO;
using KSP.OAB;
using KSP.Sim;
using KSP.Sim.Definitions;
using KSP.Sim.ResourceSystem;
using Newtonsoft.Json.Linq;
using PatchManager.Parts.Selectables;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Attributes;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.NewAssets;
using UnityEngine;

namespace PatchManager.Parts.Rulesets;

/// <summary>
/// The `:parts` ruleset used by sassy patching
/// </summary>
[PatcherRuleset("parts", "parts_data")]
public class PartsRuleset : IPatcherRuleSet
{
    /// <inheritdoc />
    public bool Matches(string label) => label == "parts_data";

    /// <summary>
    /// Converts the part json to an ISelectable following this ruleset
    /// </summary>
    /// <param name="type"></param>
    /// <param name="name">The name of the file (unused)</param>
    /// <param name="jsonData">The part json</param>
    /// <returns>An ISelectable that follows the part ruleset</returns>
    public ISelectable ConvertToSelectable(string type, string name, string jsonData)
    {
        return new PartSelectable(jsonData);
    }

    /// <summary>
    /// Create a new part asset
    /// </summary>
    /// <param name="dataValues">The arguments for the part asset, only one argument: name</param>
    /// <returns>The part asset creator</returns>
    public INewAsset CreateNew(List<DataValue> dataValues)
    {
        var name = dataValues[0].String;
        var internalPartData = new PartData
        {
            partName = name,
            angularDrag = 0,
            coMassOffset = Vector3.zero,
            coPressureOffset = Vector3.zero,
            mass = 0,
            maximumDrag = 0,
            maxTemp = 0,
            author = "",
            bodyLiftOnlyUnattachedLift = false,
            bodyLiftOnlyAttachName = "",
            buoyancy = 0,
            buoyancyUseSine = false,
            breakingForce = 0,
            breakingTorque = 0,
            category = PartCategories.none,
            family = "",
            coBuoyancy = Vector3.zero,
            coDisplacement = Vector3.zero,
            oabEditorCategory = OABEditorPartCategory.NONE,
            partType = AssemblyPartTypeFilter.Rocket,
            cost = 0,
            crashTolerance = 0,
            crewCapacity = 0,
            emissiveConstant = 0,
            explosionPotential = 0,
            fuelCrossFeed = false,
            heatConductivity = 0,
            inverseStageCarryover = false,
            isCompound = false,
            maxLength = 0,
            radiatorHeadroom = 0,
            radiatorMax = 0,
            physicsMode = PartPhysicsModes.None,
            sizeCategory = MetaAssemblySizeFilterType.XS,
            skinMassPerArea = 0,
            skinMaxTemp = 0,
            skinInternalConductionMult = 0,
            stageOffset = 0,
            stageType = AssemblyPartStageType.None,
            tags = "",
            stagingIconAssetAddress = "",
            attachRules = AttachRules.Defaults(),
            attachNodes = new List<AttachNodeDefinition>(),
            resourceContainers = new List<ContainedResourceDefinition>(),
            resourceCosts = new List<PartResourceCostDefinition>(),
            serializedPartModules = new List<SerializedPartModule>(),
            resourceSummary = new SerializedResourceInfo(),
            PAMModuleSortOverride = new List<PartsManagerCore.SerializedPartModuleDisplayOrder>(),
            PAMModuleVisualsOverride = new List<PartsManagerCore.SerializedPartModuleDisplayVisuals>(),
            AllowKinematicPhysicsIfIntersectTerrain = false
        };
        var internalJson = IOProvider.ToJson(internalPartData);
        var internalJObject = JObject.Parse(internalJson);
        var externalJObject = new JObject
        {
            ["version"] = "0.3",
            ["useExternalData"] = false,
            ["data"] = internalJObject
        };
        return new NewGenericAsset("parts_data", name, new PartSelectable(externalJObject.ToString()));
    }
}