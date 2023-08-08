using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [Header("销毁概率,[0:no, 1:all]")]
        [Range(0, 1)] public float culledRatio = 0.5f;
        public bool forceRefresh;

        [Header("Children Filters")]
        [Tooltip("object 's layer ")]
        public LayerMask layers = -1;

        [Tooltip("find children include inactive")]
        public bool includeInactive;

        [Tooltip("disable renderer when add to instance group")]
        public bool setRendererDisable = true;

        [SerializeField] public Dictionary<Mesh, InstancedGroupInfo> meshGroupDict = new Dictionary<Mesh, InstancedGroupInfo>();
        [SerializeField] public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        [HideInInspector] public Renderer[] renders;

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
            renders = rootGo.GetComponentsInChildren<MeshRenderer>(includeInactive)
                .Where(r => LayerMaskEx.Contains(layers, r.gameObject.layer))
                .ToArray();

            if (renders.Length == 0)
                return;

            AddToGroup(renders, setRendererDisable);
            groupList.AddRange(meshGroupDict.Values);

            SetupLightmaps();

            // lightmaps handle
            if (IsLightMapEnabled())
            {
                SetupGroupLightmapInfo();
            }
        }

        public void Clear()
        {
            meshGroupDict.Clear();
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


        void SetupGroupLightmapInfo()
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

                    block.SetTexture("unity_Lightmap", lightmaps[group.lightmapId]);

                    if(group.lightmapId < shadowMasks.Length)
                        block.SetTexture("unity_ShadowMask", shadowMasks[group.lightmapId]);

                    block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords.ToArray());
                    block.SetVectorArray("unity_LightmapST", lightmapGroup.lightmapCoords.ToArray());
                    block.SetInt("_DrawInstanced", 1);
                }
            }
        }

        void AddToGroup(Renderer[] renders,bool setRendererDisable)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                var r = renders[i];
                if (!r.GetComponent<MeshFilter>().sharedMesh)
                    continue;

                if (r.gameObject && setRendererDisable)
                    r.gameObject.SetActive(false);

                var info = GetInfoFromDict(r);
                info.lightmapId = r.lightmapIndex;
                info.AddRender(r.transform.localToWorldMatrix, r.lightmapScaleOffset);
            }
        }

        InstancedGroupInfo GetInfoFromDict(Renderer r)
        {
            InstancedGroupInfo info;
            var filter = r.GetComponent<MeshFilter>();
            if (!meshGroupDict.TryGetValue(filter.sharedMesh, out info))
            {
                info = new InstancedGroupInfo();
                info.mesh = filter.sharedMesh;
                info.mat = r.sharedMaterial;
                info.mat.enableInstancing = true;

                meshGroupDict.Add(filter.sharedMesh, info);
            }
            return info;
        }



        public void CullInstances(float culledRatio)
        {
            foreach (var group in groupList)
            {
                for (int i = 0; i < group.originalTransformsGroupList.Count; i++)
                {
                    var transforms = group.originalTransformsGroupList[i].transforms;
                    group.displayTransformsGroupList[i].transforms = RandomTools.Shuffle(transforms, (int)(transforms.Count * Mathf.Clamp01(culledRatio)));
                }
            }
        }

        public void DrawGroupList()
        {
            foreach (var group in groupList)
            {
                if(!group.mat)
                    continue;

                for (int i = 0; i < group.displayTransformsGroupList.Count; i++)
                {
                    var transforms = group.displayTransformsGroupList[i].transforms;

                    if(group.blockList.Count <= i)
                    {
                        group.blockList.Add(new MaterialPropertyBlock());
                    }

                    var block = group.blockList[i];

                    var shadowCasterMode = QualitySettings.shadowmaskMode == ShadowmaskMode.DistanceShadowmask ? ShadowCastingMode.On : ShadowCastingMode.Off;
                    Graphics.DrawMeshInstanced(group.mesh, 0, group.mat, transforms, block, shadowCasterMode);
                }
            }
        }

        public void UpdateGroupListMaterial(bool enableLightmap)
        {
            foreach (var groupInfo in groupList)
            {
                groupInfo.mat.SetKeyword("LIGHTMAP_ON", enableLightmap);
                groupInfo.mat.SetKeywords(new [] { "SHADOWS_SHADOWMASK", "CALCULATE_BAKED_SHADOWS" },enableLightmap);
                
            }
        }
    }
}
