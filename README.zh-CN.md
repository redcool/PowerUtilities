# PowerUtilities

[English](README.md) | [简体中文](README.zh-CN.md)

## 简介

PowerUtilities 是基于 **Unity URP 管线**的 Unity 扩展工具集，包含渲染、编辑器工具、游戏框架与 AI 实验四大类功能。它是其他 PowerXXX 着色器包（PowerLit、PowerWater、PowerVFX、PowerFur、PowerPBS 等）的公共依赖——那些包的 Shader 里大量引用了本包（PowerShaderLib 提供 HLSL include，本包提供 C# 工具、渲染特性与自定义 Shader Inspector）。下方所有模块名均对照实际源码文件验证。

## 模块（Modules）

### 渲染（Rendering）

- **BRG / BatchRendererGroup** — 高性能实例化渲染：`BRGBatch`、`BRGBatchBlock`、`BrgGroupInfo`、`BRGTools`、`DrawChildrenBRG`、着色器数据（`BRGMaterialInfo` / `BRGMaterialInfoListSO`），并附带测试场景（`TestBRG`、`TestIndirect`）。
- **SFCFramework** — 为 URP 打造的自定义 **SRP Feature/Pass 框架**（`SRPFeature`、`SRPPass`、`SRPFeatureListSO`、`SRPRenderFeatureControl`），内置大量现成 Pass：`DrawObjects`、`CreateRenderTarget`、`SetRenderTarget`（MRT 多目标渲染）、`DrawSkyBox`、`CopyColor`、`BlitToTarget`、`SetVariables`、`DepthOnly`、`DeferredLighting`、`RenderHBAO`、`RenderTAA`、`RenderMotionVector`、`DrawScreenSpaceShadow`、`MainLightShadowCaster`、`ControlURPPasses` 等；并提供 Unity 6（`_6000`）兼容变体与 UI Toolkit 图形编辑器（`Editor/SFCGraphWindow`）。示例：`Demo/Profiles/1 DrawObjects Use MRT`。
- **RenderFeatures** — URP 渲染器特性：`DrawShadow`（+Pass、`DrawShadowSettingSO`）、`PowerLitFeature`、`RenderGammaUI`（`RenderGammaUIFeature`、`RenderUIPass`，UI 伽马渲染）、`RenderTransparentObjectDepth`（透明物体深度）、`MultiPassRenderObjects`。
- **CoreTools/Rendering** — 渲染辅助：`CommandBufferEx`、`ScriptableRenderContextEx`（+_6000）、`RenderTargetHolder` / `RenderTargetInfo`、`RendererListParamsEx`、`RenderingTools`、`RenderPipelineTools`、`GraphicsBufferTools`、`LightEx`、`CameraEx`、`SHTools`、`ShaderKeywords` / `ShaderPropertyIds` / `ShaderValue`、过滤器（`SimpleFilterSetting`），以及 URP 扩展（`RTHandleTools`（+_6000）、`ScriptableRendererEx`（+_6000）、`UniversalRendererEx`、`UniversalRenderPipelineAssetEx`、`PostProcessVolumeTools`、`VolumeEx`、`ForwardLightEx` 等）。
- **GPGPU / TargetTracking** — 基于计算着色器的颜色变换（`ColorTransformCommand`）与渲染目标追踪（`RenderTargetTrackingMapControl` + `TargetTrackingCS.compute`）。
- **SRPEx** — URP 资源/光照辅助：`CustomURPAsset`、`LightBakingOutputSettings`、`URPBaseCamera`、摄像机/灯光数据编辑器扩展。
- **Instanced / StaticBatch / Batcher** — 实例化绘制辅助（`DrawChildrenInstanced*`）、静态合批（`CombineChildrenMeshGroupByMaterial`、`DrawChildrenStatic`）、编辑器分组合批（`GroupBatcher`、`MeshCombineContextMenu`）。

### 基础工具（CoreTools）

- **CSharp** — 集合扩展：`ArrayTools`、`DictionaryTools`、`LinqEx`、`ICollectionEx`、`StringBuilderTools`、`StringEx`、`ReflectionTools`、`EnumEx`、`ByteTools`、`GUIDTools`、`MD5Tools`、`ObjectEx`、`StructEx`、`RefAction`、`DelegateEx` 等。
- **Mono** — `IMono` 接口 + `MonoExecuter`（无需挂载即可执行 MonoBehaviour）。
- **PlayableTools** — Playable / Timeline 辅助：`PlayableGraphTools`、`PlayableEx`、`AnimationClipPlayableEx`、`AnimationMixerPlayableEx`。
- **EX / Math** — `ComputeShaderEx`、`RenderTextureTools`、`Texture2DEx` / `Texture3DEx` / `Texture2DArrayEx`、`AnimationTools`、`VisualElementEx`、数学辅助（`MathTools`、`MatrixEx`、`Vector4Ex`、`float4x4Ex`、`BezierCurve`）、`SingletonTools`、`UnityObjectEx`。
- **Attributes** — 检查器特性：[LoadAsset]、[ProjectSettingGroup]、[SOAssetPath]，以及一整套 EditorGUI 特性（[DisplayName]、[EnumFlags]、[EnumSearchable]、[HelpBox]、[LayerIndex]、[ShowInSceneView]、[TexturePreview]、[Tooltips] 等）与 **Group** 抽屉特性（[EditorGroup]、[EditorButton]、[EditorBox]、[EditorHeader]、[EditorIntent]、[EditorToolbar] 等）及配套 Drawer。
- **其他 CoreTools** — `SceneObjectTools`、`TransformTools`、`TerrainTools`、`MeshTools`、`MaterialGroupTools`、`ColorTools`、`LightTools`、`ShadowTools`、`GPUTools`、`ScreenTools`、`PathTools`、`RandomTools`、`CacheTools`、`CoroutineTool`、`WaitForClasses`、`CullingGroupEx`、常量类（`Layers`、`Tags`、`SortingLayers`）、`NativeArrayTools`。

### 编辑器工具（Editor）

- **EditorCoreTools** — `AssetDatabaseTools`、`EditorApplicationTools`、`EditorGUITools`、`PrefabTools`、`PresetTools`、`SelectionTools`、`SerializedObjectTools`、`PlayerSettingTools`、`ShaderEditorTools`、`TypeCacheTools`、`FBXExportTools`、`LightmapSettingTools`、`LocalUndo`（本地撤销）、`EditorBuildProcessor`、`SearchWindowTools`（搜索窗口）、ProjectSettingsAPI（`TagManager` 等）。
- **PowerShaderEditorGUI** — `PowerShaderInspector`：可配置的自定义 ShaderGUI，所有 Power* 着色器都使用它；由 **Layout/Colors/Helps/i18n** 文本配置与 [Group...] 特性系统驱动；包含 `TMP_SDFShaderGUI` 与多语言配置（`MaterialProperty_CN.txt`）。
- **MaterialDrawers（GroupAPI）** — 在全部 Power* 着色器中看到的 **[Group] 抽屉系统**：`GroupHeaderDecorator`、`GroupDecorator`，以及 enum/toggle/vector/vector-slider/texture-color/blend-mode/stencil/min-max-slider 等抽屉，还有材质装饰器（[MaterialDisableGroup]、[MaterialTooltip]、[MaterialLightInfo]）。
- **EditorTools** — 检查器扩展（`BaseEditorEx`、`AnimatorInspectorEx`、`MeshRendererEditorEx`、`TerrainInspectorEx`、`RenderTextureInspectorEx` 等）、编辑器窗口（`LightExplorerEx`、`LightmapPreviewWindowEx`、`PowerPackageManger`、`QualitySettingEx`、`PlacementWindow`、UI Toolkit 窗口：`BaseUXMLEditorWindow`、`SRPPipelineAssetWin` 等）、上下文菜单（`MaterialContextMenu`、`TextureCreateMenu`）、Project Settings 页面（`ProjectSettingsView/Settings/`：shader/材质设置、AssetBundle 编辑器工具、烘焙光照、版本最低要求检查、模板/场景标签设置等）。

### 游戏框架（Game utils）

- **GameUtilsFramework** — 摄像机（`CinemachineTools`、`TransformShakeControl`）、移动（`MovementTools`、`CameraTools`、角色控制器：`RigidbodyCharacterController`、`CharacterControllerManager`）、动画（`AnimatorEx`、`AnimationEventReceiver`、`AnimatorRootMoveRecieve`、`SetStateVariables`）、骨骼系统（`SkeletonSync`、`SkinnedMeshRendererEx`、Mixamo 导入与骨骼重定向编辑器工具）、装备（`EquipmentPartControl`）、IK（`LookAtTarget`）、俯视射击移动控制（`TopDownShooterPlayerControl`）、存档（`SaveTools`）、输入控制（`BaseInputControl` + .inputactions）。
- **InputSystem** — 输入辅助（`InputSystemTools`、`MouseDeviceTools`）；**UIEvent** — `EventSystemTools`。
- **Components** — 常用组件：`CameraLayerCull`（分层剔除）、`ComputeShaderDispatcher`、约束（`TransformConstraint`、`RectTransformConstraint`）、`SetShaderVariables`、UGUI 辅助（`TMPTextEffects`、`UGUIMaskGlobal`、`UGUIDefaultMaterialSetter`）、特效（`ImageNumberFX`）。

### Gameplay / UI / Timeline

- **GameplayAbilitySystem** — 游戏玩法标签：`GameplayAbilityTag`（SO）、`GameplayTagInfo` 及测试。
- **UIElements** — UI Toolkit 辅助：图基类（`BaseGraphView`、`BaseNodeView`、`BaseSplitView`）、`MaterialVisualElement`（在 UI Toolkit 中渲染材质）、事件接收（`UIDocumentEventRegister`）、编辑器窗口、USS/UXML 示例。
- **Timeline** — Timeline Playable：灯光控制（`LightControlTrack/Clip/Behaviour`）、字幕（`SubtitleTrack/Clip/Behaviour`）、Volume 控制（`VolumeControlTrack` + 生成的 Volume 数据）。
- **PowerGradient** — 自定义渐变资源 `PowerGradient` + 编辑器窗口/Drawer。

### 地形 / 光照贴图 / 贴图工具

- **Terrain** — `TerrainTools`、编辑器地形工具（`CustomTerrainTool`、`TerrainPathTool`、`TerrainStampControl`）、地形对齐工具、Tile 地形编辑器（`TileTerrainWindow`、`TerrainMapBaker`）。
- **Lightmap** — `BakeLightmap`（自动烘焙）、`LightmapLoader`、`LightmapInfoRecorder`、`LightProbesUpdater`、`LightProbeUniformDistribution`、编辑器画笔/窗口。
- **贴图工具** — `Material2TextureBaker`（材质烘焙为贴图）、`TexPacking`（图集打包）、`TextureChannelCombine` / `TextureChannelSplit`（通道合并/拆分）、`TextureDitherProcessor`。
- **AssetBundle** — `BundleLoader`、`ExportAssetBundle`。**Materials** — `SyncMaterialProperties`、MaterialPropCodeGen（`UIMaterialPropCodeGen` 为 UI 材质属性更新器生成代码）。

### 其他 / 性能 / AI 实验

- **Profilers** — 材质/网格合批（`MaterialBatchComp`、`MeshBatch`）、shader/材质分析窗口。
- **Performance** — `DeviceAdaptivePerformance`（自适应性能）、按质量档位激活 GameObject/粒子系统的组件；**TestUtils** — `FsrControl`（FSR 缩放控制）、`ShowFPS`、`SceneControl`、`TestShaderVariants`。
- **AICode** — AI 实验模块（详见 `AICode/README.MD`）：Ollama 客户端（`OllamaClient`）、Semantic Kernel 聊天补全（`OllamaChatCompletionService`）、Unity Sentis 测试（贴图分类、深度估计、ONNX 边缘检测 / 简易加法）。
- **TestCode** — 示例/测试：jobs、Playable、输入系统、自定义渲染纹理（水波）、CullingGroup 实例化等。**gits/** — 内置第三方包（AsciiFBXExporter）。

## 依赖与版本要求（Requirements）

Unity2022.3+ 会报错，需要修改 manifest.json：
    1. 打开 Project/Packages/manifest.json
    2. 插入以下包引用：

    类似：
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

Unity6000+ 可直接正常工作。

## 程序集定义（Assembly definitions）

`PowerUtilities.asmdef`（根）、`PowerCoreUtilities.asmdef`（CoreTools）、`PowerEditorUtilities.asmdef`（EditorCoreTools）、`AICode.asmdef`、`TestCode.asmdef`——各 shader 包（PowerLit/PowerWater/PowerVFX/PowerFur/PowerPBS 等）都引用这些程序集。
