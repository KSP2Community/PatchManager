using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using PatchManager.Planets.Selectables;

namespace PatchManager.Planets.Modifiables
{
    public class AtmosphereOverrideModifiable : JTokenModifiable
    {
        private AtmosphereOverrideSelectable _atmosphereOverrideSelectable;

        /// <summary>
        /// Creates a new <see cref="GalaxyModifiable"/> for the given <see cref="GalaxyModifiable"/>.
        /// </summary>
        /// <param name="selectable">The selectable to modify.</param>
        public AtmosphereOverrideModifiable(AtmosphereOverrideSelectable selectable) : base(selectable.AtmosphereOverrideObject, selectable.SetModified) => _atmosphereOverrideSelectable = selectable;

        /// <inheritdoc/>
        public override void Set(DataValue dataValue)
        {
            if (dataValue.IsDeletion)
            {
                _atmosphereOverrideSelectable.SetDeleted();
                return;
            }
            base.Set(dataValue);
        }
    }
}