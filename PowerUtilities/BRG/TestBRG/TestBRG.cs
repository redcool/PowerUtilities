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
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;


public class TestBRG : MonoBehaviour
{
    [EditorButton(onClickCall = "OnTest")]
    public bool isTest;

    void OnTest()
    {

    }

    public Mesh mesh;
    public Material material;

    private BatchRendererGroup m_BRG;

    private GraphicsBuffer instanceBuffer;
    private BatchID m_BatchID;
    private BatchMeshID m_MeshID;
    private BatchMaterialID m_MaterialID;

    public int numInstances = 3;
    public int updateId = 2;

    //update
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    private void Start()
    {
        m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
        m_MeshID = m_BRG.RegisterMesh(mesh);
        m_MaterialID = m_BRG.RegisterMaterial(material);

        AllocateInstanceDateBuffer();
        PopulateInstanceDataBuffer_float();

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
            objectToWorld[i] = mats[i].ToFloat3x4();
            worldToObject[i] = mats[i].inverse.ToFloat3x4();
            colors[i] = colorOffsets[i];
        }

        instanceBuffer.FillData(objectToWorld.SelectMany(m => m.ToColumnArray()).ToArray(), startByteAddressDict["unity_ObjectToWorld"], 0);
        instanceBuffer.FillData(worldToObject.SelectMany(m => m.ToColumnArray()).ToArray(), startByteAddressDict["unity_WorldToObject"], 0);
        instanceBuffer.FillData(colors.SelectMany(v => v.ToArray()).ToArray(), startByteAddressDict["_Color"], 0);
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

    List<float3x4> objectToWorld;
    List<float3x4> worldToObject;
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
        GraphicsBufferTools.FillMetadatas(dataStartIdStrides, matNames,ref metadataList,ref startByteAddressDict,ref dataStartIds);
        Debug.Log(string.Join(',', dataStartIds));
        Debug.Log(string.Join(',', startByteAddressDict.Values));

        //var graphBufferStartId = 0;
        //instanceBuffer.FillData(objectToWorld.SelectMany(m => m.ToColumnArray()).ToArray(), ref graphBufferStartId);
        //instanceBuffer.FillData(worldToObject.SelectMany(m => m.ToColumnArray()).ToArray(), ref graphBufferStartId);
        //instanceBuffer.FillData(colors.SelectMany(v => v.ToArray()).ToArray(), ref graphBufferStartId);


        for (int i = 0; i < numInstances; i++)
        {
            instanceBuffer.FillData(objectToWorld[i].ToColumnArray(), i * 12, dataStartIds[0]);
            instanceBuffer.FillData(worldToObject[i].ToColumnArray(), i * 12, dataStartIds[1]);
            instanceBuffer.FillData(colors[i].ToArray(), i * 4, dataStartIds[2]);
        }


        m_BatchID = m_BRG.AddBatch(metadataList, instanceBuffer);
        metadataList.Dispose();
    }

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        var matrices = new List<Matrix4x4>
        {
            Matrix4x4.Translate(new Vector3(-2, 0, 0)),
            Matrix4x4.Translate(new Vector3(0, 0, 0)),
            Matrix4x4.Translate(new Vector3(2, 0, 0)),
        };

        // Convert the transform matrices into the packed format that the shader expects.
        objectToWorld = new List<float3x4>
        {
            new float3x4(matrices[0].ToFloat3x4()),
            new float3x4(matrices[1].ToFloat3x4()),
            new float3x4(matrices[2].ToFloat3x4()),
        };

        // Also create packed inverse matrices.
        worldToObject = new List<float3x4>
        {
            new float3x4(matrices[0].inverse.ToFloat3x4()),
            new float3x4(matrices[1].inverse.ToFloat3x4()),
            new float3x4(matrices[2].inverse.ToFloat3x4()),
        };

        // Make all instances have unique colors.
        colors = new List<Color>
        {
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1),
        };
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
        BRGTools.DrawBatch(cullingOutput, m_BatchID, m_MaterialID, m_MeshID, numInstances);
        return new JobHandle();

    }
}
#endif