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
    public void Preload();
    
    /// <summary>
    /// Called in mod's OnInitialized method after the game is loaded.
    /// Use this to register resource locators and do things that require the GameInstance.
    /// </summary>
    public void Load();
}