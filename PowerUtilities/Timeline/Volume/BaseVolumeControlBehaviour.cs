using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace PowerUtilities.Timeline
{
    public class BaseVolumeControlBehaviour : PlayableBehaviour
    {
        [Header("Volume")]
        [Tooltip("Is valumeRef empty will use temporary Volume")]
        public ExposedReference<Volume> volumeRef;

        [Tooltip("this clip's volume,when empty use volumeRef or ower(root gameObject)'s volume")]
        [HideInInspector]
        public Volume clipVolume;

        public float volumeWeight = 1;


        public Volume GetClipVolume()
        => clipVolume;

        public virtual void UpdateVolumeSettings(Volume clipVolume) { }
        public virtual void ReadSettingsFrom(VolumeProfile vp) { }
    }
}
