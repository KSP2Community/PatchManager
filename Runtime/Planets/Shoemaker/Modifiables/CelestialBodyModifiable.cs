using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using Shoemaker.Selectables;

namespace Shoemaker.Modifiables
{
    public class CelestialBodyModifiable : JTokenModifiable
    {
        private CelestialBodySelectable _celestialBodySelectable;

        /// <summary>
        /// Creates a new <see cref="CelestialBodyModifiable"/> for the given <see cref="CelestialBodyModifiable"/>.
        /// </summary>
        /// <param name="selectable">The selectable to modify.</param>
        public CelestialBodyModifiable(CelestialBodySelectable selectable) : base(selectable.DataObject, selectable.SetModified) => _celestialBodySelectable = selectable;

        /// <inheritdoc/>
        public override void Set(DataValue dataValue)
        {
            if (dataValue.IsDeletion)
            {
                _celestialBodySelectable.SetDeleted();
                return;
            }
            base.Set(dataValue);
        }
    }
}