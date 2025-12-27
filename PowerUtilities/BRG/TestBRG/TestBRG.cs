#if UNITY_2022_2_OR_NEWER
using PowerUtilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public class TestBRG : MonoBehaviour
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

        m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
        m_MeshID = m_BRG.RegisterMesh(mesh);
        m_MaterialID = m_BRG.RegisterMaterial(material);

        AllocateInstanceDateBuffer();
        PopulateInstanceDataBuffer_float();

    }
    private void Update()
    {
        //UpdateInst(updateId);
        UpdateAll();
    }

    void UpdateInst(int id)
    {
        var mat = Matrix4x4.Translate(offsets[id]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[id];


        instanceBuffer.FillData(objectToWorld.ToColumnArray(), id * 12, startByteAddressDict["unity_ObjectToWorld"]/4);
        instanceBuffer.FillData(worldToObject.ToColumnArray(), id * 12, startByteAddressDict["unity_WorldToObject"]/4);
        instanceBuffer.FillData(color.ToArray(), id * 4, startByteAddressDict["_Color"]/4);
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

        instanceBuffer.FillData(objectToWorlds.SelectMany(m => m.ToColumnArray()).ToArray(), startByteAddressDict["unity_ObjectToWorld"]/4, 0);
        instanceBuffer.FillData(worldToObjects.SelectMany(m => m.ToColumnArray()).ToArray(), startByteAddressDict["unity_WorldToObject"]/4, 0);
        instanceBuffer.FillData(colors.SelectMany(v => v.ToArray()).ToArray(), startByteAddressDict["_Color"]/4, 0);
    }

    private void AllocateInstanceDateBuffer()
    {
        //var count = BufferCountForInstances(kBytesPerInstance, kNumInstances, kExtraBytes); // 116
        var count = BRGTools.GetByteCount(
            //(typeof(Matrix4x4), 1), // 16 * 4 =64
            (typeof(float3x4), numInstances), //12*4*3 =144
            (typeof(float3x4), numInstances), // 12*4*3
            (typeof(float4), numInstances) //16*3
            ) / sizeof(int);

        Debug.Log($"count :{count}");
        instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, sizeof(int));
    }

    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    List<Color> colors;

    // ---1
    Dictionary<string, int> startByteAddressDict = new();
    int[] dataStartIds;

    private void PopulateInstanceDataBuffer_float()
    {
        GenMaterialProperties();

        var matNames = new[]
        {
            "unity_ObjectToWorld",
            "unity_WorldToObject",
            "_Color"
        };

        var dataStartIdStrides = new[]
        {
            0,
            numInstances * 12,// objectToWorld 
            numInstances* 12, //worldToObject
            numInstances * 4 // colors
        };
        dataStartIds = new int[matNames.Length];

        var metadataList = new NativeArray<MetadataValue>(matNames.Length, Allocator.Temp);
        BRGTools.FillMetadatas(dataStartIdStrides, matNames,ref metadataList,ref startByteAddressDict,ref dataStartIds);
        Debug.Log(string.Join(',', dataStartIds));
        Debug.Log(string.Join(',', startByteAddressDict.Values));

        //var graphBufferStartId = 0;
        //instanceBuffer.FillDataBlock(objectToWorld.SelectMany(m => m.ToColumnArray()).ToArray(), ref graphBufferStartId);
        //instanceBuffer.FillDataBlock(worldToObject.SelectMany(m => m.ToColumnArray()).ToArray(), ref graphBufferStartId);
        //instanceBuffer.FillDataBlock(colors.SelectMany(v => v.ToArray()).ToArray(), ref graphBufferStartId);


        for (int i = 0; i < numInstances; i++)
        {
            instanceBuffer.FillData(objectToWorlds[i].ToColumnArray(), i * 12, dataStartIds[0]);
            instanceBuffer.FillData(worldToObjects[i].ToColumnArray(), i * 12, dataStartIds[1]);
            instanceBuffer.FillData(colors[i].ToArray(), i * 4, dataStartIds[2]);
        }


        m_BatchID = m_BRG.AddBatch(metadataList, instanceBuffer);
        metadataList.Dispose();
    }

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        var matrices = new List<Matrix4x4>();
        for (int i = 0; i < numInstances; i++)
        {
            matrices.Add(Matrix4x4.Translate(Vector3.Scale(Random.insideUnitSphere, Vector3.right) * 2));
        }

        // Convert the transform matrices into the packed format that the shader expects.
        objectToWorlds = matrices.Select(m => m.ToFloat3x4()).ToList();

        // Also create packed inverse matrices.
        worldToObjects = matrices.Select(m => m.inverse.ToFloat3x4()).ToList();

        // Make all instances have unique colors.
        colors = Enumerable.Range(0, numInstances).Select(id => Color.white * ((float)id / numInstances)).ToList();
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
        var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();
        BRGTools.SetupBatchDrawCommands(drawCmdPt, 1, numInstances);
        BRGTools.FillBatchDrawCommand(drawCmdPt, 0, m_BatchID, m_MaterialID, m_MeshID, visibleCount);
        return new JobHandle();

    }
}
#endif