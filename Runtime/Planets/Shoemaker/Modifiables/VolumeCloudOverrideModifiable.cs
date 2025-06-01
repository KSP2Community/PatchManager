using PatchManager.SassyPatching;
using PatchManager.SassyPatching.Modifiables;
using Shoemaker.Selectables;

namespace Shoemaker.Modifiables
{
    public class VolumeCloudOverrideModifiable : JTokenModifiable
    {
        private readonly VolumeCloudSelectable _volumeCloudSelectable;

        /// <summary>
        /// Creates a new <see cref="GalaxyModifiable"/> for the given <see cref="GalaxyModifiable"/>.
        /// </summary>
        /// <param name="selectable">The selectable to modify.</param>
        public VolumeCloudOverrideModifiable(VolumeCloudSelectable selectable) : base(selectable.VolumeCloudOverrideObject, selectable.SetModified) => _volumeCloudSelectable = selectable;

        /// <inheritdoc/>
        public override void Set(DataValue dataValue)
        {
            if (dataValue.IsDeletion)
            {
                _volumeCloudSelectable.SetDeleted();
                return;
            }
            base.Set(dataValue);
        }
    }
}