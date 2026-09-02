# PowerUtilities

[English](README.md) | [简体中文](README.zh-CN.md)


Unity extension & utilities collection built around the URP pipeline — rendering, editor tools, gameplay frameworks and AI experiments.

## Overview

A large utilities package used by the other PowerXXX shader packages. It contains dozens of independent modules; the main areas are described below. All module names below are verified against the actual source files.

## Modules

### Rendering

- **BRG (BatchRendererGroup)** — high-performance instanced rendering: `BRGBatch`, `BRGBatchBlock`, `BrgGroupInfo`, `BRGTools`, `DrawChildrenBRG`, shader data (`BRGMaterialInfo` / `BRGMaterialInfoListSO`), with test scenes (`TestBRG`, `TestIndirect`).
- **SFCFramework** — custom **SRP Feature/Pass** framework for URP (`SRPFeature`, `SRPPass`, `SRPFeatureListSO`, `SRPRenderFeatureControl`) with many ready passes: `DrawObjects`, `CreateRenderTarget`, `SetRenderTarget` (MRT), `DrawSkyBox`, `CopyColor`, `BlitToTarget`, `SetVariables`, `DepthOnly`, `DeferredLighting`, `RenderHBAO`, `RenderTAA`, `RenderMotionVector`, `DrawScreenSpaceShadow`, `MainLightShadowCaster`, `ControlURPPasses`, etc. — plus Unity 6 (`_6000`) variants and a UI Toolkit graph editor (`Editor/SFCGraphWindow`). Demo: `Demo/Profiles/1 DrawObjects Use MRT`.
- **RenderFeatures** — URP renderer features: `DrawShadow` (+Pass, `DrawShadowSettingSO`), `PowerLitFeature`, `RenderGammaUI` (`RenderGammaUIFeature`, `RenderUIPass`), `RenderTransparentObjectDepth`, `MultiPassRenderObjects`.
- **CoreTools/Rendering** — rendering helpers: `CommandBufferEx`, `ScriptableRenderContextEx` (+_6000), `RenderTargetHolder` / `RenderTargetInfo`, `RendererListParamsEx`, `RenderingTools`, `RenderPipelineTools`, `GraphicsBufferTools`, `LightEx`, `CameraEx`, `SHTools`, `ShaderKeywords` / `ShaderPropertyIds` / `ShaderValue`, filters (`SimpleFilterSetting`), and URP extensions (`RTHandleTools` (+_6000), `ScriptableRendererEx` (+_6000), `UniversalRendererEx`, `UniversalRenderPipelineAssetEx`, `PostProcessVolumeTools`, `VolumeEx`, `ForwardLightEx`, ...).
- **GPGPU / TargetTracking** — compute-shader color transform (`ColorTransformCommand`) and render-target tracking (`RenderTargetTrackingMapControl` + `TargetTrackingCS.compute`).
- **SRPEx** — URP asset/lighting helpers: `CustomURPAsset`, `LightBakingOutputSettings`, `URPBaseCamera`, editor extensions for camera/light data.
- **Instanced / StaticBatch / Batcher** — instanced draw helpers (`DrawChildrenInstanced*`), static batching (`CombineChildrenMeshGroupByMaterial`, `DrawChildrenStatic`), editor group batching (`GroupBatcher`, `MeshCombineContextMenu`).

### CoreTools

- **CSharp** — collection extensions: `ArrayTools`, `DictionaryTools`, `LinqEx`, `ICollectionEx`, `StringBuilderTools`, `StringEx`, `ReflectionTools`, `EnumEx`, `ByteTools`, `GUIDTools`, `MD5Tools`, `ObjectEx`, `StructEx`, `RefAction`, `DelegateEx` ...
- **Mono** — `IMono` interface + `MonoExecuter`.
- **PlayableTools** — Playable/Timeline helpers: `PlayableGraphTools`, `PlayableEx`, `AnimationClipPlayableEx`, `AnimationMixerPlayableEx`.
- **EX / Math** — `ComputeShaderEx`, `RenderTextureTools`, `Texture2DEx` / `Texture3DEx` / `Texture2DArrayEx`, `AnimationTools`, `VisualElementEx`, math helpers (`MathTools`, `MatrixEx`, `Vector4Ex`, `float4x4Ex`, `BezierCurve`), `SingletonTools`, `UnityObjectEx`.
- **Attributes** — inspector attributes: [LoadAsset], [ProjectSettingGroup], [SOAssetPath], plus an EditorGUI attribute suite ([DisplayName], [EnumFlags], [EnumSearchable], [HelpBox], [LayerIndex], [ShowInSceneView], [TexturePreview], [Tooltips]...) and the *Group* drawer attributes ([EditorGroup], [EditorButton], [EditorBox], [EditorHeader], [EditorIntent], [EditorToolbar]...) with matching drawers.
- **CoreTools misc** — `SceneObjectTools`, `TransformTools`, `TerrainTools`, `MeshTools`, `MaterialGroupTools`, `ColorTools`, `LightTools`, `ShadowTools`, `GPUTools`, `ScreenTools`, `PathTools`, `RandomTools`, `CacheTools`, `CoroutineTool`, `WaitForClasses`, `CullingGroupEx`, const classes (`Layers`, `Tags`, `SortingLayers`), `NativeArrayTools`.

### Editor

- **EditorCoreTools** — `AssetDatabaseTools`, `EditorApplicationTools`, `EditorGUITools`, `PrefabTools`, `PresetTools`, `SelectionTools`, `SerializedObjectTools`, `PlayerSettingTools`, `ShaderEditorTools`, `TypeCacheTools`, `FBXExportTools`, `LightmapSettingTools`, `LocalUndo`, `EditorBuildProcessor`, `SearchWindowTools`, ProjectSettingsAPI (`TagManager` etc.).
- **PowerShaderEditorGUI** — `PowerShaderInspector`: a configurable ShaderGUI used by all Power* shaders, driven by **Layout/Colors/Helps/i18n** txt configs and the [Group...] attribute system; includes `TMP_SDFShaderGUI` and i18n profiles (`MaterialProperty_CN.txt`).
- **MaterialDrawers (GroupAPI)** — the **[Group] drawer system** seen across all Power* shaders: `GroupHeaderDecorator`, `GroupDecorator`, drawers for enum/toggle/vector/vector-slider/texture-color/blend-mode/stencil/min-max-slider, plus material decorators ([MaterialDisableGroup], [MaterialTooltip], [MaterialLightInfo]).
- **EditorTools** — inspector extensions (`BaseEditorEx`, `AnimatorInspectorEx`, `MeshRendererEditorEx`, `TerrainInspectorEx`, `RenderTextureInspectorEx`...), editor windows (`LightExplorerEx`, `LightmapPreviewWindowEx`, `PowerPackageManger`, `QualitySettingEx`, `PlacementWindow`, UI Toolkit windows: `BaseUXMLEditorWindow`, `SRPPipelineAssetWin`...), context menus (`MaterialContextMenu`, `TextureCreateMenu`), Project Settings pages (`ProjectSettingsView/Settings/`: shader/material settings, asset bundle editor tool, bake lighting, min-version checker, stencil & scene-tag settings...).

### Game utils

- **GameUtilsFramework** — camera (`CinemachineTools`, `TransformShakeControl`), movement (`MovementTools`, `CameraTools`, characters: `RigidbodyCharacterController`, `CharacterControllerManager`), animators (`AnimatorEx`, `AnimationEventReceiver`, `AnimatorRootMoveRecieve`, `SetStateVariables`), skeleton (`SkeletonSync`, `SkinnedMeshRendererEx`, Mixamo importer & retarget editor tools), equipment (`EquipmentPartControl`), IK (`LookAtTarget`), top-down shooter player control (`TopDownShooterPlayerControl`), Save (`SaveTools`), InputControl (`BaseInputControl` + .inputactions).
- **InputSystem** — input helpers (`InputSystemTools`, `MouseDeviceTools`); **UIEvent** — `EventSystemTools`.
- **Components** — reusable components: `CameraLayerCull`, `ComputeShaderDispatcher`, constraints (`TransformConstraint`, `RectTransformConstraint`), `SetShaderVariables`, UGUI helpers (`TMPTextEffects`, `UGUIMaskGlobal`, `UGUIDefaultMaterialSetter`), FX (`ImageNumberFX`).

### Gameplay / UI / Timeline

- **GameplayAbilitySystem** — gameplay tags: `GameplayAbilityTag` (SO), `GameplayTagInfo`, tests.
- **UIElements** — UI Toolkit helpers: graph base classes (`BaseGraphView`, `BaseNodeView`, `BaseSplitView`), `MaterialVisualElement` (render a material in UI Toolkit), event receive (`UIDocumentEventRegister`), editor windows, USS/UXML samples.
- **Timeline** — Timeline playables: light control (`LightControlTrack/Clip/Behaviour`), subtitle (`SubtitleTrack/Clip/Behaviour`), volume control (`VolumeControlTrack` + generated volume data).
- **PowerGradient** — custom gradient asset `PowerGradient` + editor window/drawer.

### Terrain / Lightmap / Texture tools

- **Terrain** — `TerrainTools`, editor terrain tools (`CustomTerrainTool`, `TerrainPathTool`, `TerrainStampControl`), terrain align utilities, tile-terrain editor (`TileTerrainWindow`, `TerrainMapBaker`).
- **Lightmap** — `BakeLightmap` (auto bake), `LightmapLoader`, `LightmapInfoRecorder`, `LightProbesUpdater`, `LightProbeUniformDistribution`, editor brushes/windows.
- **Texture tools** — `Material2TextureBaker`, `TexPacking`, `TextureChannelCombine`, `TextureChannelSplit`, `TextureDitherProcessor`.
- **AssetBundle** — `BundleLoader`, `ExportAssetBundle`. **Materials** — `SyncMaterialProperties`, MaterialPropCodeGen (`UIMaterialPropCodeGen` for UI material property updater generation).

### Misc / Performance / AI experiments

- **Profilers** — material/mesh batching (`MaterialBatchComp`, `MeshBatch`), shader/material analysis windows.
- **Performance** — `DeviceAdaptivePerformance`, quality-based GameObject/ParticleSystem activators; **TestUtils** — `FsrControl`, `ShowFPS`, `SceneControl`, `TestShaderVariants`.
- **AICode** — AI experiment modules (see `AICode/README.MD`): Ollama client (`OllamaClient`), Semantic Kernel chat completion (`OllamaChatCompletionService`), Unity Sentis tests (texture classification, depth estimation, ONNX edge detection / simple add).
- **TestCode** — example/tests: jobs, playable, input system, custom render texture (water wave), culling-group instanced etc. **gits/** — vendored third-party package (AsciiFBXExporter).

## Requirements

Unity2022.3+ ,will show Errors,modify manifest.json.
    1 open Project/Packages/manifest.json
    2 insert references package,

    like:
    "dependencies": {
    "com.unity.render-pipelines.universal":"15.0.7",
    "com.unity.timeline": "1.8.2",
    "com.unity.textmeshpro": "3.0.6",
    "com.unity.inputsystem": "1.7.0",
    "com.unity.cinemachine": "3.1.3",
    "com.unity.memoryprofiler": "1.1.0",
    "com.unity.mobile.android-logcat": "1.4.2",
    "com.unity.terrain-tools": "5.0.5",
    "com.unity.splines": "2.6.1",
    "com.unity.recorder": "4.0.3",
    "com.unity.analytics": "3.8.1",
        ...
    }

Unity6000+,will normal work.

## Assembly definitions

`PowerUtilities.asmdef` (root), `PowerCoreUtilities.asmdef` (CoreTools), `PowerEditorUtilities.asmdef` (EditorCoreTools), `AICode.asmdef`, `TestCode.asmdef` — the shader packages (PowerLit/PowerWater/PowerVFX/PowerFur/PowerPBS...) reference these assemblies.