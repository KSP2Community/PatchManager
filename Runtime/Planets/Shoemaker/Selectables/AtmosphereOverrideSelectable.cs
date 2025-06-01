using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using Shoemaker.Modifiables;

namespace Shoemaker.Selectables
{
    public sealed class AtmosphereOverrideSelectable : BaseSelectable
    {
    #pragma warning disable CS0414 // Field is assigned but its value is never used
        private bool _modified;
    #pragma warning restore CS0414 // Field is assigned but its value is never used
        private bool _deleted;

        /// <summary>
        /// Marks this part selectable as having been modified any level down
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
            _deleted = true;
        }

        public readonly JObject AtmosphereOverrideObject;

        public AtmosphereOverrideSelectable(JObject atmosphereOverrideObject)
        {
            Children = new List<ISelectable>();
            Classes = new List<string>();
            ElementType = "atmosphere_override";
            AtmosphereOverrideObject = atmosphereOverrideObject;
            Name = atmosphereOverrideObject["PlanetName"].Value<string>();
            foreach (var (key, child) in atmosphereOverrideObject)
            {
                Classes.Add(key);
            }
        }
        
        /// <inheritdoc />
        public override List<ISelectable> Children { get; }

        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override List<string> Classes { get; }

        public override bool MatchesClass(string @class, out DataValue classValue) =>
            throw new Exception("Class capture selectors are not allowed on atmosphere overrides");

        /// <inheritdoc />
        public override bool IsSameAs(ISelectable other) =>
            other is AtmosphereOverrideSelectable atmosphereOverrideSelectable && atmosphereOverrideSelectable.AtmosphereOverrideObject == AtmosphereOverrideObject;
        
        /// <inheritdoc />
        public override IModifiable OpenModification() => new AtmosphereOverrideModifiable(this);

        public override ISelectable AddElement(string elementType) =>
            throw new Exception("Adding elements to atmosphere overrides is not allowed");
        /// <inheritdoc />
        public override string Serialize() => _deleted ? "" : AtmosphereOverrideObject.ToString();

        /// <inheritdoc />
        public override DataValue GetValue() => DataValue.FromJToken(AtmosphereOverrideObject);
        public override string ElementType { get; }
    }
}

