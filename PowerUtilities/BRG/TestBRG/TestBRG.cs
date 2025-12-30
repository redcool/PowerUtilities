#if UNITY_2022_2_OR_NEWER
using NUnit.Framework;
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public partial class TestBRG : MonoBehaviour
{

    public Mesh mesh;
    public Material material;

    private BatchRendererGroup m_BRG;

    private GraphicsBuffer instanceBuffer;
    private BatchID m_BatchID;
    private BatchMeshID m_MeshID;
    private BatchMaterialID m_MaterialID;

    public int numInstances = 3;
    public int visibleCount = 1;
    public int updateId = 2;

    //update
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    private void Start()
    {
        offsets = new Vector3[numInstances];
        colorOffsets = new Color[numInstances];
        for (int i = 0; i < numInstances; i++)
        {
            offsets[i] = Random.insideUnitSphere * 10;
            colorOffsets[i] = Random.ColorHSV();
        }

        m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
        m_MeshID = m_BRG.RegisterMesh(mesh);
        m_MaterialID = m_BRG.RegisterMaterial(material);

        GenMaterialProperties();

        GenInstanceDateBuffer();
        FillInstanceDataBuffer();
    }
    private void Update()
    {
        UpdateInst(updateId);
        //UpdateAll();
    }

    void UpdateInst(int id)
    {
        var mat = Matrix4x4.Translate(offsets[id]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[id];

        objectToWorlds[0] = objectToWorld;
        worldToObjects[0] = worldToObject;
        colors[0] = color;

        instanceBuffer.SetData(objectToWorlds, 0, GetDataStartId("unity_ObjectToWorld", 12, id,1), 1);
        instanceBuffer.SetData(worldToObjects, 0, GetDataStartId("unity_WorldToObject", 12, id,1), 1);
        instanceBuffer.SetData(colors, 0, GetDataStartId("_Color", 4, id,1), 1);

        //instanceBuffer.SetData(objectToWorld.ToColumnArray(), 0, GetDataStartId("unity_ObjectToWorld", 1, id,12), 12);
        //instanceBuffer.SetData(worldToObject.ToColumnArray(), 0, GetDataStartId("unity_WorldToObject", 1, id,12), 12);
        //instanceBuffer.SetData(color.ToArray(), 0, GetDataStartId("_Color", 1, id,4), 4);
    }

    private void UpdateAll()
    {
        var mats = new Matrix4x4[numInstances];
        for (int i = 0; i < numInstances; i++)
        {
            mats[i] = Matrix4x4.Translate(offsets[i]);
            objectToWorlds[i] = mats[i].ToFloat3x4();
            worldToObjects[i] = mats[i].inverse.ToFloat3x4();
            colors[i] = colorOffsets[i];
        }

        instanceBuffer.SetData(objectToWorlds, 0, GetDataStartId("unity_ObjectToWorld", 12), objectToWorlds.Count);
        instanceBuffer.SetData(worldToObjects, 0, GetDataStartId("unity_WorldToObject", 12), numInstances);
        instanceBuffer.SetData(colors, 0, GetDataStartId("_Color", 4), numInstances);
    }

    private void GenInstanceDateBuffer()
    {
        var count = BRGTools.GetByteCount(numInstances,
            typeof(float3x4),
            typeof(float3x4),
            typeof(float4));
        count /= 4;
        //var count = BRGTools.GetByteCount(
        //    //(typeof(Matrix4x4), 1), // 16 * 4 =64
        //    (typeof(float3x4), numInstances), //12*4*3 =144
        //    (typeof(float3x4), numInstances), // 12*4*3
        //    (typeof(float4), numInstances) //16*3
        //    ) / sizeof(int);

        Assert.AreEqual(count, (12+12+4) * numInstances);

        Debug.Log($"all instance mat float count :{count} floats");
        instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, sizeof(int));
    }
    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    List<Color> colors;

    // {propName, startid = float count offset index}
    Dictionary<string, int> startIdDict = new();

    public int GetDataStartId(string matPropName,int elementFloatCount=1,int instanceId=0,int elementCount=1)
    {
        return startIdDict[matPropName] / elementFloatCount + instanceId* elementCount;
    }

    private void FillInstanceDataBuffer()
    {
        var matPropInfos = new[]
        {
            ("unity_ObjectToWorld",12),
            ("unity_WorldToObject",12),
            ("_Color",4),
        };

        var metadataList = new NativeArray<MetadataValue>(matPropInfos.Length, Allocator.Temp);
        BRGTools.SetupMetadatas(numInstances, matPropInfos,ref metadataList,startIdDict);

        Debug.Log("startIdDict : " + string.Join(',', startIdDict.Values));//0,12*instCount,24*instCount

        instanceBuffer.SetData(objectToWorlds, 0, GetDataStartId("unity_ObjectToWorld",12), objectToWorlds.Count);
        instanceBuffer.SetData(worldToObjects, 0, GetDataStartId("unity_WorldToObject",12), numInstances);
        instanceBuffer.SetData(colors, 0, GetDataStartId("_Color",4), numInstances);

        m_BatchID = m_BRG.AddBatch(metadataList, instanceBuffer);
        metadataList.Dispose();
    }

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        var matrices = new List<Matrix4x4>();
        colors = new List<Color>();
        for (int i = 0; i < numInstances; i++)
        {
            matrices.Add(Matrix4x4.Translate(offsets[i]));
            colors.Add(colorOffsets[i]);
        }

        // Convert the transform matrices into the packed format that the shader expects.
        objectToWorlds = matrices.Select(m => m.ToFloat3x4()).ToList();

        // Also create packed inverse matrices.
        worldToObjects = matrices.Select(m => m.inverse.ToFloat3x4()).ToList();
    }

    private void OnDisable()
    {
        instanceBuffer.Dispose();
        m_BRG.Dispose();
    }

    public unsafe JobHandle OnPerformCulling(
        BatchRendererGroup rendererGroup,
        BatchCullingContext cullingContext,
        BatchCullingOutput cullingOutput,
        IntPtr userContext)
    {
        var drawCommands = new BatchCullingOutputDrawCommands();
        drawCommands.drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(sizeof(BatchDrawCommand), 16, Allocator.TempJob);
        drawCommands.drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(sizeof(BatchDrawRange), 16, Allocator.TempJob);
        drawCommands.visibleInstances = (int*)UnsafeUtility.Malloc(sizeof(int) * numInstances, 16, Allocator.TempJob);

        drawCommands.drawCommandCount = 1;
        drawCommands.drawRangeCount = 1;

        drawCommands.visibleInstanceCount = numInstances;
        for (int i = 0;i<numInstances;i++)
            drawCommands.visibleInstances[i] = i;

        drawCommands.drawCommands[0] = new BatchDrawCommand()
        {
            visibleCount = (uint)numInstances,
            visibleOffset = 0,
            batchID = m_BatchID,
            meshID = m_MeshID,
            materialID = m_MaterialID,
            submeshIndex = 0,
            splitVisibilityMask = 0xff,
            flags = BatchDrawCommandFlags.None
        };
        drawCommands.drawRanges[0] = new BatchDrawRange
        {
            drawCommandsType = BatchDrawCommandType.Direct,
            filterSettings = new BatchFilterSettings
            {
                allDepthSorted = true,
                renderingLayerMask = 1,
                layer = 0,
                
            },
            drawCommandsCount = 1,
            drawCommandsBegin = 0
        };
        cullingOutput.drawCommands[0] = drawCommands;
        

        //var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();
        //BRGTools.SetupBatchDrawCommands(drawCmdPt, 1, numInstances);
        //BRGTools.FillBatchDrawCommand(drawCmdPt, 0, m_BatchID, m_MaterialID, m_MeshID, visibleCount);
        return new JobHandle();

    }
}
#endif