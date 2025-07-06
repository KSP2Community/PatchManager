using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Planets.Modifiables;
using PatchManager.Planets.Overrides;

namespace PatchManager.Planets.Selectables
{
    public sealed class VolumeCloudSelectable : BaseSelectable
    {
        private bool _modified;
        public override bool WasModified => false;
        public override void ClearModified()
        {
            _modified = false;
        }

        public bool Deleted;

        /// <summary>
        /// Marks this part selectable as having been modified any level down///
        /// </summary>
        public void SetModified()
        { 
            _modified = true;
        }

        /// <summary>
        /// Marks this part as goneso
        /// </summary>
        public void SetDeleted()
        {
            SetModified();
            Deleted = true;
        }

        public readonly JObject VolumeCloudOverrideObject;

        public VolumeCloudSelectable(JObject volumeCloudOverrideObject)
        {
            Children = new List<ISelectable>();
            Classes = new List<string>();
            ElementType = "volume_cloud_override";
            VolumeCloudOverrideObject = volumeCloudOverrideObject;
            Name = volumeCloudOverrideObject["bodyName"].Value<string>();
            foreach (var (key, child) in volumeCloudOverrideObject)
            {
                Classes.Add(key);
            }

            foreach (var child in (JArray)volumeCloudOverrideObject["cumulusList"])
            {
                var obj = (JObject)child;
                var name = obj["layerName"]!.Value<string>();
                Classes.Add(name);
                Children.Add(new JTokenSelectable(SetModified, child, name, "cloud_data"));
            }
        }
    
        /// <inheritdoc />
        public override List<ISelectable> Children { get; }

        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override List<string> Classes { get; }

        public override bool MatchesClass(string @class, out DataValue classValue)
        {
            foreach (var value in (JArray)VolumeCloudOverrideObject["cumulusList"]!)
            {
                if (((JObject)value)["layerName"]!.Value<string>() != @class) continue;
                classValue = DataValue.FromJToken(value);
                return true;
            }

            classValue = DataValue.Null;
            return false;
        }

        /// <inheritdoc />
        public override bool IsSameAs(ISelectable other) =>
            other is VolumeCloudSelectable volumeCloudSelectable && volumeCloudSelectable.VolumeCloudOverrideObject == VolumeCloudOverrideObject;
    
        /// <inheritdoc />
        public override IModifiable OpenModification() => new VolumeCloudOverrideModifiable(this);

        public override ISelectable AddElement(string elementType)
        {
            SetModified();
            var newLayer = new CloudsDataOverride
            {
                layerName = elementType
            };
            var obj = JObject.FromObject(newLayer);
            ((JArray)VolumeCloudOverrideObject["cumulusList"])!.Add(obj);
            var selectable = new JTokenSelectable(SetModified, obj, elementType, "cloud_data");
            Children.Add(selectable);
            Classes.Add(elementType);
            return selectable;
        }
        /// <inheritdoc />
        public override string Serialize() => Deleted ? "" : VolumeCloudOverrideObject.ToString();

        /// <inheritdoc />
        public override DataValue GetValue() => DataValue.FromJToken(VolumeCloudOverrideObject);
        public override string ElementType { get; }
    }
}

