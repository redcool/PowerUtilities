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

        [Header("销毁概率,[0:no, 1:all]")]
        [Range(0, 1)] public float culledRatio = 0f;
        public bool forceRefresh;

        [Header("Children Filters")]
        [Tooltip("object 's layer ")]
        public LayerMask layers = -1;

        [Tooltip("find children include inactive")]
        public bool includeInactive;

        [Header("When done")]
        [Tooltip("disable children when add to instance group")]
        public bool disableChildren = true;

        [Space(10)]
        public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        [HideInInspector] public Renderer[] renders;
        /// <summary>
        /// index of all DrawChildrenInstanced
        /// </summary>
        public int drawChildrenId;

        public void Update()
        {
            if (forceRefresh)
            {
                forceRefresh = false;

                CullInstances(1 - culledRatio);
            }
            var isLightmapOn = IsLightMapEnabled();
            UpdateGroupListMaterial(isLightmapOn);

            if (isLightmapOn)
            {
                SetupGroupLightmapInfo();
            }
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
            SetupGroupList(renders, groupList);
            SetupLightmaps();

            // lightmaps handle
            if (IsLightMapEnabled())
            {
                SetupGroupLightmapInfo();
            }

            if (disableChildren)
                rootGo.SetChildrenActive(false);

            // refresh culling
            forceRefresh = true; 
        }

        /// <summary>
        /// find valid renders
        /// </summary>
        /// <param name="rootGo"></param>
        /// <param name="includeInactive"></param>
        /// <param name="layers"></param>
        /// <param name="renders"></param>
        public void SetupRenderers(GameObject rootGo)
        {
            var q = rootGo.GetComponentsInChildren<MeshRenderer>(includeInactive)
                .Where(r =>
                {
                    var isValid = LayerMaskEx.Contains(layers, r.gameObject.layer);
                    isValid = isValid && r.sharedMaterial;

                    if (isValid && !r.sharedMaterial.enableInstancing)
                        r.sharedMaterial.enableInstancing = true;

                    var mf = r.GetComponent<MeshFilter>();
                    return isValid && mf && mf.sharedMesh;
                });

            renders = q.ToArray();
        }

        /// <summary>
        /// Fill GroupList with renders
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="groupList"></param>
        public static void SetupGroupList(Renderer[] renders, List<InstancedGroupInfo> groupList)
        {
            // use lightmapIndex,mesh as group
            var lightmapMeshGroups = renders.GroupBy(r => new { r.lightmapIndex, r.GetComponent<MeshFilter>().sharedMesh });
            lightmapMeshGroups.ForEach((group, groupId) =>
            {
                // a instanced group
                var groupInfo = new InstancedGroupInfo();
                groupList.Add(groupInfo);

                group.ForEach((r, renderId) =>
                {
                    var boundSphereSize = Mathf.Max(r.bounds.extents.x, Mathf.Max(r.bounds.extents.y, r.bounds.extents.z));
                    groupInfo.AddRender(boundSphereSize, r.GetComponent<MeshFilter>().sharedMesh,r.sharedMaterial, r.transform.localToWorldMatrix, r.lightmapScaleOffset, r.lightmapIndex);
                });
            });
        }

        public void Clear()
        {
            groupList.Clear();
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

        public void SetupGroupLightmapInfo()
        {
            foreach (var group in groupList)
            {
                for (int i = 0; i < group.lightmapCoordsList.Count; i++)
                {
                    if(group.blockList.Count <= i)
                    {
                        group.blockList.Add(new MaterialPropertyBlock());
                    }
                    var block = group.blockList[i];
                    var lightmapGroup = group.lightmapCoordsList[i];

                    if (IsLightmapValid(group.lightmapId, lightmaps))
                    {
                        block.SetTexture("unity_Lightmap", lightmaps[group.lightmapId]);
                    }

                    if (IsLightmapValid(group.lightmapId, shadowMasks))
                        block.SetTexture("unity_ShadowMask", shadowMasks[group.lightmapId]);

                    block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords.ToArray());
                    block.SetVectorArray("unity_LightmapST", lightmapGroup.lightmapCoords.ToArray());
                    block.SetInt("_DrawInstanced", 1);

                    if(isSubstractiveMode)
                        block.SetVector("unity_LightData", Vector4.zero);
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

        public void DrawGroupList()
        {
            var transforms = new List<Matrix4x4>();

            var shadowCasterMode = QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? ShadowCastingMode.On : ShadowCastingMode.Off;
            groupList.ForEach((group, groupId) =>
            {
                //if (!group.mat)
                //    return;

                group.originalTransformsGroupList.ForEach((segment, sid) =>
                {
                    transforms.Clear();
                    segment.transformVisibleList.ForEach((tr, trId) =>
                    {
                        if (segment.transformVisibleList[trId] && segment.transformShuffleCullingList[trId])
                            transforms.Add(segment.transforms[trId]);
                    });

                    //transforms = group.displayTransformsGroupList[i].transforms;
                    //if (group.blockList.Count <= i)
                    //{
                    //    group.blockList.Add(new MaterialPropertyBlock());
                    //}

                    var block = group.blockList[sid];

                    Graphics.DrawMeshInstanced(group.mesh, 0, group.mat, transforms, block, shadowCasterMode);
                });
            });
        }

        public void UpdateGroupListMaterial(bool enableLightmap)
        {
            foreach (var groupInfo in groupList)
            {
                var lightmapEnable = groupInfo.lightmapId == -1 && enableLightmap;

                if(groupInfo.mat.IsKeywordEnabled("LIGHTMAP_ON") != lightmapEnable)
                    groupInfo.mat.SetKeyword("LIGHTMAP_ON", lightmapEnable);

                //groupInfo.mat.SetKeywords(new [] { "SHADOWS_SHADOWMASK"},enableLightmap);
                
            }
        }

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
    }
}
