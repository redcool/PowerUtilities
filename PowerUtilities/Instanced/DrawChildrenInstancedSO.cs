using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// draw instanced config
    /// </summary>
    public class DrawChildrenInstancedSO : ScriptableObject
    {
        [Header("Lightmap")]
        public Texture2D[] lightmaps;
        public Texture2D[] shadowMasks;
        public bool enableLightmap = true;
        [Tooltip("Lighting Mode is subtractive?")]
        public bool isSubstractiveMode = false;

        [Header("Lightmap Array")]
        public Texture2DArray lightmapArray;
        public bool isEnableLightmapArray;

        [Header("Find Children Filters")]
        [Tooltip("object 's layer ")]
        public LayerMask layers = -1;

        [Tooltip("find children include inactive")]
        public bool includeInactive;
        public string excludeTag = Tags.EditorOnly;

        [Tooltip("< this,use srp batch")]
        public int groupMinCount = 30;
        

        [Header("When done")]
        [Tooltip("disable children when add to instance group")]
        public bool disableChildren = true;


        [Header("销毁概率,[0:no, 1:all]")]
        [Range(0, 1)] public float culledRatio = 0f;
        public bool forceRefresh;

        [Header("Draw Options")]
        [Tooltip("which layer to use")]
        [LayerIndex]public int layer = 0;
        public bool receiveShadow = true;
        [Tooltip("which camera can draw,null will drawn in all cameras ")]
        public Camera camera = null;
        [Tooltip("ShadowMaskMode is DistanceShadowMask and shadowCastMode.On,will draw shadow")]
        public ShadowCastingMode shadowCastMode = ShadowCastingMode.Off;
        public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

        [Space(10)]
        [Header("Draw Datas")]
        public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        [HideInInspector] public Renderer[] renders;
        /// <summary>
        /// index of all DrawChildrenInstanced
        /// </summary>
        public int drawChildrenId;

        static MaterialPropertyBlock block;

        public void Update()
        {
            if (forceRefresh)
            {
                CullInstances(1 - culledRatio);
                ResetGroupListMaterial();

                forceRefresh = false;
            }
        }

        public void SetRendersActive(bool isActive)
        {
            renders.ForEach(r=> r.enabled = isActive);
        }

        public bool IsLightMapEnabled()
        {
            return enableLightmap && lightmaps != null && lightmaps.Length > 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootGo"></param>
        public void SetupChildren(GameObject rootGo)
        {
            // fitler children
            SetupRenderers(rootGo);

            if (renders.Length == 0)
                return;

            groupList.Clear();
            List<Renderer> removedRenderList = null;
            if (isEnableLightmapArray)
            {
                removedRenderList = SetupGroupListByMesh(renders, groupList,this);
            }
            else
            {
                removedRenderList = SetupGroupList(renders, groupList, this);
            }
            // remove dont instanced renders
            renders = renders.Where(r => !removedRenderList.Contains(r)).ToArray();

            SetupLightmaps();
            
            if (disableChildren)
                SetRendersActive(false);

            // refresh culling
            forceRefresh = true; 
        }

        /// <summary>
        /// find valid renders,
        /// (layer,tag( exclude EditorOnly,
        /// need sharedMaterial,sharedMesh
        /// </summary>
        /// <param name="rootGo"></param>
        /// <param name="includeInactive"></param>
        /// <param name="layers"></param>
        /// <param name="renders"></param>
        public void SetupRenderers(GameObject rootGo,bool useExistRenders=false)
        {
            if (useExistRenders)
            {
                if (renders.Where(r => r).Count() > 0)
                    return;
            }

            var q = rootGo.GetComponentsInChildren<MeshRenderer>(includeInactive)
                .Where(r =>
                {
                    // layer
                    var isValid = LayerMaskEx.Contains(layers, r.gameObject.layer);
                    // exclude EditorOnly
                    if (!string.IsNullOrEmpty(excludeTag))
                    {
                        isValid = isValid && !r.gameObject.CompareTag(excludeTag);
                    }
                    // material
                    isValid = isValid && r.sharedMaterial;

                    // open isntaced
                    //if (isValid && !r.sharedMaterial.enableInstancing)
                    //    r.sharedMaterial.enableInstancing = true;

                    var mf = r.GetComponent<MeshFilter>();
                    return isValid && mf && mf.sharedMesh;
                });

            renders = q.ToArray();
        }
            
        public class GroupLightmapMeshMaterial
        {
            public int lightmapIndex;
            public Mesh mesh;
            public Material material;
        }

        /// <summary>
        /// group renders by(lgihtmapIndex,sharedMesh,shaderMaterial)
        /// fill groupList
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="groupList"></param>
        public static List<Renderer> SetupGroupList(Renderer[] renders, List<InstancedGroupInfo> groupList,DrawChildrenInstancedSO inst)
        {
            // use lightmapIndex,mesh as group
            var lightmapMeshGroups = renders.GroupBy(r => new { r.lightmapIndex, r.GetComponent<MeshFilter>().sharedMesh, r.sharedMaterial });
            return FillGroupList(lightmapMeshGroups, groupList, inst);
        }
        public static List<Renderer> SetupGroupListByMesh(Renderer[] renders, List<InstancedGroupInfo> groupList, DrawChildrenInstancedSO inst)
        {
            // use lightmapIndex,mesh as group
            var meshGroups = renders.GroupBy(r => new { r.GetComponent<MeshFilter>().sharedMesh, r.sharedMaterial });
            return FillGroupList(meshGroups, groupList, inst);
        }

        public static List<Renderer> FillGroupList(IEnumerable<IGrouping<object, Renderer>> lightmapMeshGroups, List<InstancedGroupInfo> groupList, DrawChildrenInstancedSO inst)
        {
            var removedRenders = new List<Renderer>();
            //var sb = new StringBuilder();
            lightmapMeshGroups.ForEach((group, groupId) =>
            {
                //sb.AppendLine(groupId+":"+ group.Count());
                if (group.Count() < inst.groupMinCount)
                {
                    //sb.AppendLine("dont instanced "+groupId);
                    group.ForEach(r => {
                        r.enabled = true;
                        r.sharedMaterial.enableInstancing = true;
                        removedRenders.Add(r);
                    });
                    return;
                }
                // a instanced group
                var groupInfo = new InstancedGroupInfo();
                groupList.Add(groupInfo);

                group.ForEach((r, renderId) =>
                {
                    var boundSphereSize = Mathf.Max(r.bounds.extents.x, Mathf.Max(r.bounds.extents.y, r.bounds.extents.z));
                    groupInfo.AddRender(boundSphereSize, r.GetComponent<MeshFilter>().sharedMesh, r.sharedMaterial, r.transform.localToWorldMatrix, r.lightmapScaleOffset, r.lightmapIndex);
                });
            });
            //Debug.Log(sb);
            return removedRenders;
        }

        public void Clear()
        {
            groupList.Clear();
        }
        public void ResetGroupListMaterial()
        {
            groupList.ForEach((group, groupId) =>
            {
                group.Reset();
            });
        }

        public void DestroyOrHiddenChildren(bool destroyGameObject)
        {
            foreach (var item in renders)
            {
                // destroy items;
                if (destroyGameObject)
                {
#if UNITY_EDITOR
                    DestroyImmediate(item.gameObject);
#else
                    Destroy(item.gameObject);
#endif
                }
                else
                    item.gameObject.SetActive(false);
            }
        }

        public void SetupLightmaps()
        {
            lightmaps = new Texture2D[LightmapSettings.lightmaps.Length];
            shadowMasks = new Texture2D[LightmapSettings.lightmaps.Length];

            LightmapSettings.lightmaps.ForEach((lightmapData, id) =>
            {
                lightmaps[id] = lightmapData.lightmapColor;
                shadowMasks[id] = lightmapData.shadowMask;
            });
        }

        public static bool IsLightmapValid(int lightmapId, Texture[] lightmaps)
        => lightmapId >-1 && lightmapId< lightmaps.Length && lightmaps[lightmapId];

        void UpdateSegmentBlock(InstancedGroupInfo group, Vector4[] lightmapCoords, MaterialPropertyBlock block)
        {
            // lightmap_ST
            if (lightmapCoords.Length >0)
            {
                //block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords);
                //block.SetInt("_DrawInstanced", 1);
                block.SetVectorArray("unity_LightmapST", lightmapCoords);
            }
            // for substractive mode
            if (isSubstractiveMode)
                block.SetVector("unity_LightData", Vector4.zero);

            // use texture Array
            if (isEnableLightmapArray)
            {
                group.mat.EnableKeyword("UNITY_INSTANCED_LIGHTMAPSTS");
                block.SetTexture("unity_Lightmaps", lightmapArray!=null ? (Texture)lightmapArray : Texture2D.blackTexture);
                block.SetFloatArray("unity_LightmapIndexArray", group.lightmapIdList);
            }
            else // use textures
            {
                group.mat.DisableKeyword("UNITY_INSTANCED_LIGHTMAPSTS");
                if (IsLightmapValid(group.lightmapId, shadowMasks))
                {
                    block.SetTexture("unity_ShadowMask", shadowMasks[group.lightmapId]);
                }

                if (IsLightmapValid(group.lightmapId, lightmaps))
                {
                    block.SetTexture("unity_Lightmap", lightmaps[group.lightmapId]);
                }
            }
        }

        /// <summary>
        /// update group.displayTransformsGroupList,whith culledRatio
        /// </summary>
        /// <param name="culledRatio"></param>
        public void CullInstances(float culledRatio)
        {
            //foreach (var group in groupList)
            groupList.ForEach((group, groupId) =>
            {
                for (int i = 0; i < group.originalTransformsGroupList.Count; i++)
                {
                    var transforms = group.originalTransformsGroupList[i].transforms;
                    // clear all
                    group.originalTransformsGroupList[i].transformShuffleCullingList.SetData(false);

                    // set visible
                    var retainCount = (int)(transforms.Count * Mathf.Clamp01(culledRatio));
                    var shuffleIds = RandomTools.GetShuffle(retainCount);
                    shuffleIds.ForEach(shuffleId =>
                    {
                        group.originalTransformsGroupList[i].transformShuffleCullingList[shuffleId] = true;
                    });
                }
            });
        }

        // objs can visible
        Matrix4x4[] visibleTransforms = new Matrix4x4[1023];
        Vector4[] visibleLightmapSTs = new Vector4[1023];

        public void DrawGroupList(CommandBuffer cmd = null)
        {
            ShadowCastingMode shadowCasterMode = GetShadowCastingMode();

            //foreach (var group in groupList)
            for (int groupId = 0; groupId < groupList.Count; groupId++)
            {
                var group = groupList[groupId];

                //update material LIGHTMAP_ON
                var lightmapEnable = group.lightmapId != -1 && IsLightMapEnabled();
                LightmapSwitch(group.mat, lightmapEnable);

                var transformsGroupList = group.originalTransformsGroupList.Count;
                for (int sid = 0; sid<transformsGroupList; ++sid)
                {
                    var segment = group.originalTransformsGroupList[sid];
                    var visibleCount = 0;
                    var visibleListCount = segment.transformVisibleList.Count;

                    for (int trId = 0; trId < visibleListCount; trId++)
                    {
                        if (segment.transformVisibleList[trId] && segment.transformShuffleCullingList[trId])
                        {
                            visibleTransforms[visibleCount] = (segment.transforms[trId]);
                            visibleLightmapSTs[visibleCount]=(group.lightmapCoordsList[sid].lightmapCoords[trId]);

                            visibleCount++;
                        }
                    };

                    if (visibleCount == 0)
                        break;

                    if (block == null)
                        block = new MaterialPropertyBlock();

                    UpdateSegmentBlock(group, visibleLightmapSTs, block);

                    DrawInstanced(shadowCasterMode, visibleCount, group);

                    block.Clear();
                };
            };

            void DrawInstanced(ShadowCastingMode shadowCasterMode, int visibleCount, InstancedGroupInfo group)
            {
                if (cmd != null)
                {
                    cmd.DrawMeshInstanced(group.mesh, 0, group.mat, 0, visibleTransforms, visibleCount, block);
                }
                else
                {
                    Graphics.DrawMeshInstanced(group.mesh, 0, group.mat, visibleTransforms, visibleCount, block, shadowCasterMode,
                        receiveShadow, layer, camera, lightProbeUsage);
                }
            }
        }

        private ShadowCastingMode GetShadowCastingMode()
        {
            return (QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask && shadowCastMode!= ShadowCastingMode.Off)
                ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }

        private static void LightmapSwitch(Material mat, bool lightmapEnable)
        {
            if (mat.IsKeywordEnabled("LIGHTMAP_ON") != lightmapEnable)
                mat.SetKeyword("LIGHTMAP_ON", lightmapEnable);
        }
#if _DEBUG
        public override string ToString()
        {
            var sb = new StringBuilder();

            groupList.ForEach((group, groupId) =>
            {
                sb.AppendLine(groupId.ToString());

                for (int i = 0; i < group.originalTransformsGroupList.Count; i++)
                {
                    var instancedGroup = group.originalTransformsGroupList[i];

                    sb.AppendLine("tr group : "+i);
                    sb.AppendLine(instancedGroup.ToString());
                }
            });

            return sb.ToString();
        }
#endif
    }
}
