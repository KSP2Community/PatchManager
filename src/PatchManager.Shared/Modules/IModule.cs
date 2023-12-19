using JetBrains.Annotations;
using SpaceWarp.API.Configuration;
using UnityEngine.UIElements;

namespace PatchManager.Shared.Modules;

/// <summary>
/// Base interface for PatchManager DLL modules.
/// </summary>
public interface IModule
{
    /// <summary>
    /// Called in the mod's Start method before the game is loaded.
    /// Use this method to register actions with the FlowManager.
    /// </summary>
    public void Init();
    
    /// <summary>
    /// Called in mod's OnPreInitialized method after the game is loaded.
    /// Use this to register resource locators and do things that require the GameInstance.
    /// </summary>
    public void PreLoad();
    
    /// <summary>
    /// Called in mod's OnInitialized method after the game is loaded.
    /// Use this to register resource locators and do things that require the GameInstance.
    /// </summary>
    public void Load();

    /// <summary>
    /// Called when getting the details information for patch manager after game load.
    /// </summary>
    /// <returns>A visual element describing information about the module or null for no information</returns>
    [CanBeNull]
    public VisualElement GetDetails();

    /// <summary>
    /// Called to bind configuration to this module specifically.
    /// </summary>
    /// <param name="modConfiguration">The configuration of the base Patch Manager mod to bind to</param>
    public void BindConfiguration(IConfigFile modConfiguration);
}