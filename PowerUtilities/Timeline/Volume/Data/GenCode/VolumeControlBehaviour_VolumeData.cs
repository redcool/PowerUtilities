///
/// Generated Code
/// UI : ProjectSettings/PowerUtils/PostControlCodeGen
/// paste this file to PowerUtilities\PowerUtilities\Timeline\Volume\Data\GenCode
///

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace PowerUtilities.Timeline
{
    public partial class VolumeControlBehaviour//VolumeControlBehaviour
    {
        // setting variables ,like public Test_Bloom_Data bloomData;
        [Header("VolumeControlBehaviour")]
        public Bloom_Data _Bloom_Data;
public ChannelMixer_Data _ChannelMixer_Data;
public ChromaticAberration_Data _ChromaticAberration_Data;
public ColorAdjustments_Data _ColorAdjustments_Data;
public ColorCurves_Data _ColorCurves_Data;
public ColorLookup_Data _ColorLookup_Data;
public DepthOfField_Data _DepthOfField_Data;
public FilmGrain_Data _FilmGrain_Data;
public LensDistortion_Data _LensDistortion_Data;
public LiftGammaGain_Data _LiftGammaGain_Data;
public MotionBlur_Data _MotionBlur_Data;
public PaniniProjection_Data _PaniniProjection_Data;
public ShadowsMidtonesHighlights_Data _ShadowsMidtonesHighlights_Data;
public SplitToning_Data _SplitToning_Data;
public Tonemapping_Data _Tonemapping_Data;
public Vignette_Data _Vignette_Data;
public WhiteBalance_Data _WhiteBalance_Data;


        public override void UpdateVolumeSettings(Volume clipVolume)
        {
            if (!clipVolume)
                return;

            // if(bloomData.isEnable)
            //     VolumeDataTools.Update(clipVolume, bloomData);
            base.UpdateVolumeSettings(clipVolume);
              VolumeDataTools.Update(clipVolume, _Bloom_Data);
  VolumeDataTools.Update(clipVolume, _ChannelMixer_Data);
  VolumeDataTools.Update(clipVolume, _ChromaticAberration_Data);
  VolumeDataTools.Update(clipVolume, _ColorAdjustments_Data);
  VolumeDataTools.Update(clipVolume, _ColorCurves_Data);
  VolumeDataTools.Update(clipVolume, _ColorLookup_Data);
  VolumeDataTools.Update(clipVolume, _DepthOfField_Data);
  VolumeDataTools.Update(clipVolume, _FilmGrain_Data);
  VolumeDataTools.Update(clipVolume, _LensDistortion_Data);
  VolumeDataTools.Update(clipVolume, _LiftGammaGain_Data);
  VolumeDataTools.Update(clipVolume, _MotionBlur_Data);
  VolumeDataTools.Update(clipVolume, _PaniniProjection_Data);
  VolumeDataTools.Update(clipVolume, _ShadowsMidtonesHighlights_Data);
  VolumeDataTools.Update(clipVolume, _SplitToning_Data);
  VolumeDataTools.Update(clipVolume, _Tonemapping_Data);
  VolumeDataTools.Update(clipVolume, _Vignette_Data);
  VolumeDataTools.Update(clipVolume, _WhiteBalance_Data);

        }

        public override void ReadSettingsFrom(VolumeProfile vp)
        {
            base.ReadSettingsFrom(vp);
        }

    }

}