using System.Collections.Generic;
using PatchManager.Core;
using PatchManager.Generic;
using PatchManager.Missions;
using PatchManager.Parts;
using PatchManager.Parts.Selectables;
using PatchManager.Resources;
using PatchManager.Science;
using PatchManager.Shared;
using PatchManager.Shared.Modules;
using Redux.ExtraModTypes;
using ReduxLib.Configuration;
using SpaceWarp.API.Mods;
using UnityEngine;
using UnityEngine.UIElements;
using ILogger = ReduxLib.Logging.ILogger;

namespace PatchManager
{
    public class PatchManager : KerbalMod
    {
        internal static PatchManager Instance;
        public void Awake()
        {
            // Let's register all our modules!
            Instance = this;
            ModuleManager.Register(typeof(CoreModule));
            ModuleManager.Register(typeof(GenericModule));
            ModuleManager.Register(typeof(MissionsModule));
            ModuleManager.Register(typeof(PartsModule));
            ModuleManager.Register(typeof(ResourcesModule));
            ModuleManager.Register(typeof(ScienceModule));
            Logging.Initialize(SWLogger);
            foreach (var module in ModuleManager.Modules)
            {
                // We are going to use reduxes core configuration
                module.BindConfiguration(SWConfiguration);
            }
            ModuleManager.InitAll();
        }

        public override void OnPreInitialized()
        {
            ModuleManager.PreLoadAll();
        }

        public override void OnInitialized()
        {
            ModuleManager.LoadAll();
        }

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

            SpaceWarp.UI.API.ModList.RegisterDetailsFoldoutGenerator("PatchManager", GeneratePatchManagerText);
        }
    }
}