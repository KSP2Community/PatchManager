using System.Collections.Generic;
using KSP.Sim;
using Newtonsoft.Json.Linq;
using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Interfaces;
using PatchManager.SassyPatching.Selectables;
using PatchManager.Planets.Modifiables;

namespace PatchManager.Planets.Selectables
{
    public sealed class GalaxySelectable : BaseSelectable
    {
        private bool _modified;
        public override bool WasModified => _modified;
        public override void ClearModified()
        {
            _modified = false;
        }

        public bool Deleted;

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
            Deleted = true;
        }

        /// <summary>
        /// This is the object that represents the galaxy definition
        /// </summary>
        public readonly JObject GalaxyObject;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="galaxyObject"></param>
        /// <param name="elementType"></param>
        public GalaxySelectable(JObject galaxyObject, string elementType)
        {
            GalaxyObject = galaxyObject;
            ElementType = elementType;
            Name = galaxyObject["Name"].Value<string>();
            Children = new List<ISelectable>();
            Classes = new List<string>();
            var bodies = (JArray)GalaxyObject["CelestialBodies"];
            foreach (var jToken in bodies)
            {
                var body = (JObject)jToken;
                var name = body["GUID"]!.Value<string>();
                Classes.Add(name);
                Children.Add(new JTokenSelectable(SetModified, body, token => ((JObject)token).Value<string>(), "planet"));
            }
        }

        /// <inheritdoc />
        public override List<ISelectable> Children { get; }

        /// <inheritdoc />
        public override string Name { get; }

        /// <inheritdoc />
        public override List<string> Classes { get; }

        /// <inheritdoc />
        public override bool MatchesClass(string @class, out DataValue classValue)
        {
            var bodies = (JArray)GalaxyObject["CelestialBodies"];
            foreach (var jToken in bodies!)
            {
                var body = (JObject)jToken;
                var name = body["GUID"]!.Value<string>();
                if (name != @class)
                {
                    continue;
                }
                classValue = DataValue.FromJToken(body);
                return true;
            }
            classValue = DataValue.Null;
            return false;
        }

        /// <inheritdoc />
        public override bool IsSameAs(ISelectable other) =>
            other is GalaxySelectable galaxySelectable && galaxySelectable.GalaxyObject == GalaxyObject;

        /// <inheritdoc />
        public override IModifiable OpenModification() => new GalaxyModifiable(this);

        /// <inheritdoc />
        public override ISelectable AddElement(string elementType)
        {
            SetModified();
            var obj = new SerializedCelestialBody
            {
                GUID = elementType,
                OrbitProperties = new SerializedOrbitProperties(),
                OrbiterProperties = new SerializedOribiterDefinition()
            };
            var jToken = JObject.FromObject(obj);
            Classes.Add(elementType);
            var selectable = new JTokenSelectable(SetModified, jToken, token => ((JObject)token).Value<string>(), "planet");
            Children.Add(selectable);
            return selectable;
        }

        /// <inheritdoc />
        public override string Serialize() => Deleted ? "" : GalaxyObject.ToString();

        /// <inheritdoc />
        public override DataValue GetValue() => DataValue.FromJToken(GalaxyObject);

        /// <inheritdoc />
        public override string ElementType { get; }
    }
}

