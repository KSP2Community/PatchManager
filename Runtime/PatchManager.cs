using System.Collections.Generic;
using PatchManager.Core;
using PatchManager.CSharpPatching;
using PatchManager.Generic;
using PatchManager.LuaPatching;
using PatchManager.Missions;
using PatchManager.Parts;
using PatchManager.Planets;
using PatchManager.Resources;
using PatchManager.Science;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using Redux.ExtraModTypes;
using UnityEngine.UIElements;

namespace PatchManager
{
    /// <summary>
    /// Main PatchManager mod entry point: registers every PatchManager submodule and drives them through SpaceWarp's lifecycle.
    /// </summary>
    public class PatchManager : KerbalMod
    {
        /// <summary>
        /// Singleton instance, set during <see cref="Awake" />.
        /// </summary>
        internal static PatchManager Instance;

        /// <summary>
        /// Registers each PatchManager submodule, binds shared configuration, and runs the modules' <see cref="IModule.Init" /> hooks.
        /// </summary>
        public void Awake()
        {
            Instance = this;
            ModuleManager.Register(typeof(LuaPatchingModule));
            ModuleManager.Register(typeof(CoreModule));
            ModuleManager.Register(typeof(GenericModule));
            ModuleManager.Register(typeof(MissionsModule));
            ModuleManager.Register(typeof(PartsModule));
            ModuleManager.Register(typeof(ResourcesModule));
            ModuleManager.Register(typeof(ScienceModule));
            ModuleManager.Register(typeof(PlanetsModule));
            ModuleManager.Register(typeof(CSharpPatchingModule));
            Logging.Initialize(SWLogger);
            foreach (var module in ModuleManager.Modules)
            {
                // Bind against Redux's core configuration.
                module.BindConfiguration(SWConfiguration);
            }
            ModuleManager.InitAll();
        }

        /// <inheritdoc />
        public override void OnPreInitialized()
        {
            ModuleManager.PreLoadAll();
        }

        /// <inheritdoc />
        public override void OnInitialized()
        {
            ModuleManager.LoadAll();
        }

        /// <inheritdoc />
        public override void OnPostInitialized()
        {
            InitializePatchManagerDetailsFoldout();
        }

        private static void InitializePatchManagerDetailsFoldout()
        {
            VisualElement GeneratePatchManagerText()
            {
                var detailsContainer = new ScrollView();
                var str = "Loaded modules: ";
                var toAdd = new List<VisualElement>();
                foreach (var module in ModuleManager.Modules)
                {
                    str += $"\n- {module.GetType().Name}";
                    var details = module.GetDetails();
                    if (details != null)
                    {
                        toAdd.Add(details);
                    }
                }

                str += "\n";
                var loadedModules = new TextElement
                {
                    text = str,
                    visible = true,
                    style =
                    {
                        display = DisplayStyle.Flex
                    }
                };
                detailsContainer.Add(loadedModules);
                foreach (var element in toAdd)
                {
                    detailsContainer.Add(element);
                }

                detailsContainer.visible = true;
                detailsContainer.style.display = DisplayStyle.Flex;
                return detailsContainer;
            }

            SpaceWarp2.UI.API.ModList.RegisterDetailsFoldoutGenerator("PatchManager", GeneratePatchManagerText);
        }
    }
}
