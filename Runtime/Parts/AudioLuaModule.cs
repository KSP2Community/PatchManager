using MoonSharp.Interpreter;
using Newtonsoft.Json.Linq;
using PatchManager.LuaPatching;
using PatchManager.LuaPatching.Attributes;
using PatchManager.LuaPatching.Builtin;
using PatchManager.Parts.UserData;

namespace PatchManager.Parts
{
    [PatchManagerModule("Audio")]
    [MoonSharpUserData]
    public class AudioLuaModule
    {
        public AudioLuaModule(PatchManagerCore pmc, Universe universe)
        {
        }

        public void EnsurePreset(
            PartUserData part,
            string presetId,
            string targetTransformPath = null,
            bool overrideExistingAudio = false)
        {
            if (part == null)
            {
                throw new ScriptRuntimeException("PM.Audio:EnsurePreset requires a part.");
            }

            if (string.IsNullOrWhiteSpace(presetId))
            {
                throw new ScriptRuntimeException("PM.Audio:EnsurePreset requires a preset ID.");
            }

            part.EnsureModule("Module_PartAudioPreset", module =>
            {
                module.EnsureDataObject("PartAudioPreset", dataObject =>
                {
                    if (dataObject["Presets"] is not JArray presets)
                    {
                        presets = new JArray();
                        dataObject["Presets"] = presets;
                    }

                    string normalizedTarget = targetTransformPath ?? string.Empty;
                    foreach (JToken preset in presets)
                    {
                        if (preset is not JObject presetObject)
                        {
                            continue;
                        }

                        string existingId = presetObject.Value<string>("PresetId");
                        string existingTarget = presetObject.Value<string>("TargetTransformPath") ?? string.Empty;
                        if (existingId == presetId && existingTarget == normalizedTarget)
                        {
                            presetObject["OverrideExistingAudio"] = overrideExistingAudio;
                            return;
                        }
                    }

                    presets.Add(new JObject
                    {
                        ["PresetId"] = presetId,
                        ["TargetTransformPath"] = targetTransformPath,
                        ["OverrideExistingAudio"] = overrideExistingAudio
                    });
                });
            });
        }
    }
}
