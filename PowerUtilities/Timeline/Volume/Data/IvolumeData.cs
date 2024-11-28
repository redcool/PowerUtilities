
namespace PowerUtilities
{
    using System;
    using UnityEngine.Rendering;

    public interface IvolumeData
    {
        public Type ComponentType {  get; }
        public void UpdateSetting(VolumeComponent vc);
        public void RecordSetting(VolumeComponent vc);
    }

    public class BaseVolumeData<T>
    {
        public T setting;
    }
}
