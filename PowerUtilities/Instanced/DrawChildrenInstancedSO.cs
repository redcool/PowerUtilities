using GameUtilsFramework;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [Header("Find Children Filters")]
        [Tooltip("object 's layer ")]
        public LayerMask layers = -1;

        [Tooltip("find children include inactive")]
        public bool includeInactive;
        public string excludeTag = Tags.EditorOnly;


        [Header("When done")]
        [Tooltip("disable children when add to instance group")]
        public bool disableChildren = true;

        [Header("销毁概率,[0:no, 1:all]")]
        [Range(0, 1)] public float culledRatio = 0f;
        public bool forceRefresh;

        [Header("Draw Options")]
        [Tooltip("which layer to use")]
        [LayerIndex] public int layer = 0;
        public bool receiveShadow = true;
        [Tooltip("which camera can draw,null will drawn in all cameras ")]
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
#endif
        public string cameraTag = Tags.MainCamera;

        [Tooltip("ShadowMaskMode is DistanceShadowMask and shadowCastMode.On,will draw shadow")]
        public ShadowCastingMode shadowCasterMode = ShadowCastingMode.Off;
        public LightProbeUsage lightProbeUsage = LightProbeUsage.BlendProbes;

        public Bounds worldBounds = new Bounds(Vector3.zero, new Vector3(5000, 5000, 5000));

        [Space(10)]
        [Header("Draw Datas")]
        public int allInstanceCount = 0; // groupList total instance count
        public List<InstancedGroupInfo> groupList = new List<InstancedGroupInfo>();

        [HideInInspector] public Renderer[] renders;

        Camera cam;
        public Camera GetCamera()
        {
            if (!cam)
                cam = CameraTools.GetCamera(cameraTag);
            return cam;
        }

        public void Update()
        {
            if (forceRefresh)
            {
                ResetGroupListMaterial();

                forceRefresh = false;
            }
        }

        public void SetRendersActive(bool isActive)
        {
            renders.ForEach(r => r.enabled = isActive);
        }

        public bool IsLightMapEnabled(int lightmapId)
        {
            return enableLightmap
                && lightmapId != -1
                && lightmaps != null
                && lightmaps.Length > 0;
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

            allInstanceCount = SetupGroupList(renders, groupList, this);

            DrawChildrenInstancedTools.SetupLightmaps(ref lightmaps, ref shadowMasks);

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
        public void SetupRenderers(GameObject rootGo, bool useExistRenders = false)
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

                    var mf = r.GetComponent<MeshFilter>();
                    return isValid && mf && mf.sharedMesh;
                });

            renders = q.ToArray();
        }

        /// <summary>
        /// group renders by(lgihtmapIndex,sharedMesh,shaderMaterial)
        /// fill groupList
        /// </summary>
        /// <param name="renders"></param>
        /// <param name="groupList"></param>
        public static int SetupGroupList(Renderer[] renders, List<InstancedGroupInfo> groupList, DrawChildrenInstancedSO inst)
        {
            var groups = renders.GroupBy(r => new { r.lightmapIndex, r.GetComponent<MeshFilter>().sharedMesh, r.sharedMaterial });
            var allInstanceCount = 0;
            foreach (var group in groups)
            {
                var subGroups = group.Chunk(1023);
                foreach (var subGroup in subGroups)
                {
                    var instanceCount = subGroup.Count();
                    var groupInfo = new InstancedGroupInfo(instanceCount);
                    groupList.Add(groupInfo);

                    groupInfo.lightmapId = group.Key.lightmapIndex;
                    groupInfo.mesh = group.Key.sharedMesh;
                    groupInfo.originalMat = group.Key.sharedMaterial;
                    groupInfo.globalOffset = allInstanceCount;

                    allInstanceCount += instanceCount;

                    var renderers = subGroup.ToArray();
                    // a instanced group
                    for (int i = 0; i < instanceCount; i++)
                    {
                        var renderer = renderers[i];

                        groupInfo.transforms[i] = renderer.transform.localToWorldMatrix;
                        groupInfo.lightmapCoords[i] = renderer.lightmapScaleOffset;

                        var boundSphereSize = renderer.bounds.extents.sqrMagnitude;
                        var center = renderer.bounds.center;
                        groupInfo.boundingSpheres[i] = new Vector4(center.x, center.y, center.z, boundSphereSize);
                    }
                }
            }
            return allInstanceCount;
        }

        public void Clear()
        {
            foreach (var group in groupList)
            {
                group.Clear();
            }
            groupList.Clear();
        }
        public void ResetGroupListMaterial()
        {
            foreach (var group in groupList)
            {
                group.Reset();
            }
        }

        public void DestroyOrHiddenChildren(bool destroyGameObject)
        {
            foreach (var item in renders)
            {
                // destroy items;
                if (destroyGameObject)
                {
                    item.gameObject.Destroy();
                }
                else
                    item.gameObject.SetActive(false);
            }
        }

    }
}
