using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// draw instanced config
    /// </summary>
    public class DrawChildrenInstancedSO : ScriptableObject
    {
        public Texture2D[] lightmaps;
        public bool enableLightmap = true;
        public bool destroyGameObjectsWhenBaked = true;

        [Header("销毁概率")]
        [Range(0, 1)] public float culledRatio = 0.5f;
        public bool forceRefresh;

        [SerializeField] public Dictionary<Mesh, InstancedGroupInfo> meshGroupDict = new Dictionary<Mesh, InstancedGroupInfo>();
        [SerializeField] public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        Renderer[] renders;

        public void Update()
        {
            if (forceRefresh)
            {
                forceRefresh = false;

                CullInstances(1 - culledRatio);
                UpdateGroupListMaterial(IsLightMapEnabled());
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
            renders = rootGo.GetComponentsInChildren<MeshRenderer>(true);
            if (renders.Length == 0)
                return;

            AddToGroup(renders);
            groupList.AddRange(meshGroupDict.Values);

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
            renders = null;

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
                    block.SetVectorArray("_LightmapST", lightmapGroup.lightmapCoords.ToArray());
                    block.SetInt("_DrawInstanced", 1);

                }
            }
        }

        void AddToGroup(Renderer[] renders)
        {
            for (int i = 0; i < renders.Length; i++)
            {
                var r = renders[i];
                if (r.gameObject)
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
                for (int i = 0; i < group.displayTransformsGroupList.Count; i++)
                {
                    var transforms = group.displayTransformsGroupList[i].transforms;

                    if(group.blockList.Count <= i)
                    {
                        group.blockList.Add(new MaterialPropertyBlock());
                    }

                    var block = group.blockList[i];

                    Graphics.DrawMeshInstanced(group.mesh, 0, group.mat, transforms, block);
                }
            }
        }

        public void UpdateGroupListMaterial(bool enableLightmap)
        {
            foreach (var groupInfo in groupList)
            {
                var needUpdate = enableLightmap != groupInfo.mat.IsKeywordEnabled("LIGHTMAP_ON");
                if (!needUpdate)
                    continue;

                if (enableLightmap)
                    groupInfo.mat.EnableKeyword("LIGHTMAP_ON");
                else
                    groupInfo.mat.DisableKeyword("LIGHTMAP_ON");
            }
        }
    }
}
