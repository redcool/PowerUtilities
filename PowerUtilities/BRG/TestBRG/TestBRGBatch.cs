#if UNITY_2022_2_OR_NEWER
using PowerUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;


public class TestBRGBatch : MonoBehaviour
{
    //public Mesh mesh;
    public Material material;
    public Mesh mesh;


    [Header("Count")]
    public int groupCount = 1;
    public int numInstancesPerGroup = 3;

    [Header("Update inst")]
    public bool isUpdateInstance;
    public int updateId = 2;
    public int updateMeshId = 0;
    //update
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    private BatchRendererGroup brg;
    List<BRGBatch> brgBatches = new();
    int numInstances;

    private void Awake()
    {

        if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
        {
            var bytes = BatchRendererGroup.GetConstantBufferMaxWindowSize();
            numInstancesPerGroup = bytes / (32 * 4);
            Debug.Log($"-------- device :{bytes}, insts : {numInstancesPerGroup}");
        }

    }

    private void Start()
    {
        numInstances = groupCount * numInstancesPerGroup;

        offsets = new Vector3[numInstances];
        colorOffsets = new Color[numInstances];

        GenMaterialProperties();

        brg = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);


        var matId = brg.RegisterMaterial(material);
        var meshId = brg.RegisterMesh(mesh);

        // find material props
        var matPropInfoList = new List<(string name, int floatCount)>();
        var floatsCount = 0;
        material.shader.FindShaderPropNames_BRG(ref matPropInfoList, ref floatsCount, isSkipTexST: true);
        var matPropInfoArr = matPropInfoList.ToArray();

        for (int j = 0; j < groupCount; j++)
        {
            var brgBatch = new BRGBatch(brg, numInstancesPerGroup, meshId, matId, j);
            brgBatches.Add(brgBatch);

            brgBatch.Setup(floatsCount, matPropInfoArr);
            //brgBatch.Setup(12 + 12 + 4 + 4,
            //    new[] {
            //        (BRGTools.unity_ObjectToWorld,12),
            //        (BRGTools.unity_WorldToObject,12),
            //        //("_MainTex_ST",4),
            //        (BRGTools._Color,4),
            //    });

            // 1 setdata per instance
            for (int i = 0; i < numInstancesPerGroup; i++)
            {
                var objectToWorld = objectToWorlds[i + j * numInstancesPerGroup];
                var worldToObject = worldToObjects[i + j * numInstancesPerGroup];
                var color = colors[i + j * numInstancesPerGroup];
                color = Color.red;
                var mainTexST = new Vector4(1, 1, 0, 0);

                var startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12, i, 1);
                brgBatch.instanceBuffer.SetData(objectToWorld, 0, startId);

                startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12, i, 1);
                brgBatch.instanceBuffer.SetData(worldToObject, 0, startId);

                startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools._Color, 4, i, 1);
                brgBatch.instanceBuffer.SetData(color, 0, startId);
            }
            // 2 setdata per batch(group)
            //var instId = numInstancesPerGroup * j;
            //brgBatch.instanceBuffer.SetData(objectToWorlds, instId, BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12), numInstancesPerGroup);
            //brgBatch.instanceBuffer.SetData(worldToObjects, instId, BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12), numInstancesPerGroup);
            //brgBatch.instanceBuffer.SetData(colors, instId, BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools._Color, 4), numInstancesPerGroup);
        }

    }

    private void Update()
    {
        if (isUpdateInstance)
        {
            UpdateInst(updateId, updateMeshId);
        }
        //UpdateAll();
    }

    void UpdateInst(int instId, int batchId)
    {
        instId = Mathf.Clamp(instId, 0, numInstancesPerGroup - 1);

        var instIdGlobal = instId + batchId * numInstancesPerGroup;
        var mat = Matrix4x4.Translate(offsets[instIdGlobal]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[instIdGlobal];

        var brgBatch = brgBatches[batchId];

        var startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12, instId, 1);
        brgBatch.instanceBuffer.SetData(objectToWorld, 0, startId);

        startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12, instId, 1);
        brgBatch.instanceBuffer.SetData(worldToObject, 0, startId);

        startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools._Color, 4, instId, 1);
        brgBatch.instanceBuffer.SetData(color, 0, startId);
    }

    private void UpdateAll()
    {
        var id = 0;
        for (int i = 0; i < numInstancesPerGroup; i++)
        {
            matrices[id] = Matrix4x4.Translate(offsets[id] + Vector3.right * i);
            objectToWorlds[id] = matrices[id].ToFloat3x4();
            worldToObjects[id] = matrices[id].inverse.ToFloat3x4();
            colors[id] = colorOffsets[id];

            id++;
        }

        for (int i = 0; i < brgBatches.Count; i++)
        {
            var brgBatch = brgBatches[i];

            var startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_ObjectToWorld, 12);
            brgBatch.instanceBuffer.SetData(objectToWorlds, 0, startId, objectToWorlds.Count);

            startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools.unity_WorldToObject, 12);
            brgBatch.instanceBuffer.SetData(worldToObjects, 0, startId, worldToObjects.Count);

            startId = BRGTools.GetDataStartId(brgBatch.propNameStartFloatIdDict, BRGTools._Color, 4);
            brgBatch.instanceBuffer.SetData(colors, 0, startId, colors.Count);

        }
    }


    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    public List<Color> colors;

    List<Matrix4x4> matrices;
    public float modelScale = 1;
    public Vector3 itemOffset;

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        matrices = new List<Matrix4x4>();

        for (int i = 0; i < numInstances; i++)
        {
            //matrices.Add(Matrix4x4.TRS(Random.insideUnitSphere * 10,Quaternion.identity,Vector3.one * modelScale));
            var y = i / 100;
            var x = i % 100;
            var offset = new Vector3(x * itemOffset.x, 0, y * itemOffset.z) + transform.position;
            matrices.Add(Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * modelScale));
        }

        // Convert the transform matrices into the packed format that the shader expects.
        objectToWorlds = matrices.Select(m => m.ToFloat3x4()).ToList();

        // Also create packed inverse matrices.
        worldToObjects = matrices.Select(m => m.inverse.ToFloat3x4()).ToList();

        // Make all instances have unique colors.
        colors = Enumerable.Range(0, numInstances).Select(id => Random.ColorHSV()).ToList();
    }

    private void OnDisable()
    {
        foreach (var brgBatch in brgBatches)
        {
            brgBatch.Dispose();
        }
        brg.Dispose();
    }

    public unsafe JobHandle OnPerformCulling(
        BatchRendererGroup rendererGroup,
        BatchCullingContext cullingContext,
        BatchCullingOutput cullingOutput,
        IntPtr userContext)
    {
        var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

        var numInstances = brgBatches.Sum(b => b.numInstances);
        BRGTools.SetupBatchDrawCommands(drawCmdPt, brgBatches.Count, numInstances);
        foreach (var batch in brgBatches)
        {
            batch.DrawBatch(drawCmdPt);
        }

        return new JobHandle();
    }

}
#endif