namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Profiling;
    using UnityEngine.Rendering;

    [ExecuteInEditMode]
    public class DrawChildrenInstanced : MonoBehaviour
    {
        [Tooltip("Bake children from ")]
        public GameObject rootGO;

        [EditorSettingSO]
        public DrawChildrenInstancedSO drawInfoSO;
        DrawChildrenInstancedSO lastDrawInfoSo; 

        public CullingGroup cullingGroup;
        static MaterialPropertyBlock block;

        public void Start()
        {
            enabled = GraphicsDeviceTools.IsDeviceSupportInstancing();
        }

        private void OnEnable()
        {
            TryInitCullingGroup();
        }

        void Update()
        {
            if (CompareTools.CompareAndSet(ref lastDrawInfoSo, drawInfoSO))
            {
                if (drawInfoSO)
                    SetupBoundingSpheres(cullingGroup, drawInfoSO.allInstanceCount, drawInfoSO.groupList);
            }

            drawInfoSO?.Update();

        }
        void LateUpdate()
        {
            Profiler.BeginSample("Draw Children Instanced");
            if (drawInfoSO && cullingGroup != null)
            {
                DrawGroupList(null);
            }
            Profiler.EndSample();
        }

        private void OnDisable()
        {
            CullingGroupEx.DestroySafe(ref cullingGroup);
        }

        public void SetupDrawInfo()
        {
            drawInfoSO.Clear();
            drawInfoSO.SetupChildren(rootGO ? rootGO : gameObject);
        }

        public void TryInitCullingGroup()
        {
            if (cullingGroup != null)
                return;

            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = Camera.main;

            if (drawInfoSO)
                SetupBoundingSpheres(cullingGroup, drawInfoSO.allInstanceCount, drawInfoSO.groupList);
        }

        public void SetupBoundingSpheres(CullingGroup cullingGroup,int allInstanceCount,List<InstancedGroupInfo> groupList)
        {
            var spheres = new BoundingSphere[allInstanceCount];
            var count = 0;
            foreach (var group in groupList)
            {
                if (group.boundingSpheres == null)
                    continue;

                for (int i = 0; i < group.boundingSpheres.Length; i++)
                {
                    var bs = group.boundingSpheres[i];
                    spheres[count++] = new BoundingSphere(new Vector3(bs.x, bs.y, bs.z), bs.w);
                }
            }
            // setup bounding spheres
            cullingGroup.SetBoundingSpheres(spheres);
            cullingGroup.SetBoundingSphereCount(count);
        }

        // objs can visible
        Matrix4x4[] visibleTransforms = new Matrix4x4[1023];
        Vector4[] visibleLightmapSTs = new Vector4[1023];
        int[] visibleIndices = new int[1023];


        public void DrawGroupList(CommandBuffer cmd)
        {
            if (block == null)
                block = new MaterialPropertyBlock();

            //foreach (var group in groupList)
            for (int groupId = 0; groupId < drawInfoSO.groupList.Count; groupId++)
            {
                block.Clear();

                var group = drawInfoSO.groupList[groupId];
                var lightmapEnable = drawInfoSO.IsLightMapEnabled(group.lightmapId);

                var lightmapCoords = lightmapEnable ? null : visibleLightmapSTs;
                var visibleCount = group.instanceCount;
                visibleCount = group.UpdateVisibles(cullingGroup, visibleTransforms, visibleLightmapSTs, visibleIndices);
                if (visibleCount == 0)
                    continue;

                //update material LIGHTMAP_ON
                DrawChildrenInstancedTools.LightmapSwitch(group.mat, lightmapEnable);
                DrawChildrenInstancedTools.UpdateLightmap(group.mat, block, group.lightmapId, drawInfoSO.lightmaps, drawInfoSO.shadowMasks, visibleLightmapSTs, drawInfoSO.isSubstractiveMode);

                DrawInstanced(cmd, group.mesh, group.mat, ref visibleTransforms, visibleCount);
            }

        }

        void DrawInstanced(CommandBuffer cmd, Mesh mesh, Material mat, ref Matrix4x4[] visibleTransforms, int visibleCount)
        {
            if (cmd != null)
            {
                cmd.DrawMeshInstanced(mesh, 0, mat, 0, visibleTransforms, visibleCount, block);
            }
            else
            {
                var renderParams = new RenderParams(mat)
                {
                    layer = drawInfoSO.layer,
#if !UNITY_EDITOR
                    camera = drawInfoSO.GetCamera() ?? Camera.main,
#endif
                    receiveShadows = drawInfoSO.receiveShadow,
                    lightProbeUsage = drawInfoSO.lightProbeUsage,
                    shadowCastingMode = DrawChildrenInstancedTools.GetShadowCastingMode(drawInfoSO.shadowCasterMode),
                    worldBounds = drawInfoSO.worldBounds,
                    matProps = block,
                };

                Graphics.RenderMeshInstanced(renderParams, mesh, 0, visibleTransforms, visibleCount, 0);
            }
        }
    }
}