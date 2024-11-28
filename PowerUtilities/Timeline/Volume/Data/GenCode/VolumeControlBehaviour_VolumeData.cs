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

namespace PowerUtilities.Timeline
{
    public partial class VolumeControlBehaviour
    {
        // setting variables ,like public Test_Bloom_Data bloomData;

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


        public void UpdateVolumeSettings()
        {
            if (!clipVolume)
                return;

            // if(bloomData.isEnable)
            //     VolumeDataTools.Update(clipVolume, bloomData);
            if(_Bloom_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _Bloom_Data);
if(_ChannelMixer_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ChannelMixer_Data);
if(_ChromaticAberration_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ChromaticAberration_Data);
if(_ColorAdjustments_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ColorAdjustments_Data);
if(_ColorCurves_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ColorCurves_Data);
if(_ColorLookup_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ColorLookup_Data);
if(_DepthOfField_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _DepthOfField_Data);
if(_FilmGrain_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _FilmGrain_Data);
if(_LensDistortion_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _LensDistortion_Data);
if(_LiftGammaGain_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _LiftGammaGain_Data);
if(_MotionBlur_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _MotionBlur_Data);
if(_PaniniProjection_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _PaniniProjection_Data);
if(_ShadowsMidtonesHighlights_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _ShadowsMidtonesHighlights_Data);
if(_SplitToning_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _SplitToning_Data);
if(_Tonemapping_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _Tonemapping_Data);
if(_Vignette_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _Vignette_Data);
if(_WhiteBalance_Data.isEnable) 
  VolumeDataTools.Update(clipVolume, _WhiteBalance_Data);

        }

        public void ReadSettingsFrom(VolumeProfile vp)
        {

        }

    }

}