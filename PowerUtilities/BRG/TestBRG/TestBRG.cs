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
        PopulateInstanceDataBuffer_float_span();

    }
    private void Update()
    {
        UpdateInst(updateId);
    }

    void UpdateInst(int id)
    {
        var mat = Matrix4x4.Translate(offsets[id]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var colors = colorOffsets[id];

        
        instanceBuffer.Update(objectToWorld.ToColumnArray().ToList(), startByteAddressDict["unity_ObjectToWorld"] + id * GraphicsBufferTools.FLOAT3X4_BYTES);
        instanceBuffer.Update(worldToObject.ToColumnArray().ToList(), startByteAddressDict["unity_WorldToObject"] + id * GraphicsBufferTools.FLOAT3X4_BYTES);
        instanceBuffer.Update(colors.ToArray().ToList(), startByteAddressDict["_Color"] + id * GraphicsBufferTools.FLOAT4_BYTES);
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

        instanceBuffer.Update(objectToWorld.SelectMany(m => m.ToColumnArray()).ToList(), startByteAddressDict["unity_ObjectToWorld"]);
        instanceBuffer.Update(worldToObject.SelectMany(m => m.ToColumnArray()).ToList(), startByteAddressDict["unity_WorldToObject"]);
        instanceBuffer.Update(colors.SelectMany(v => v.ToArray()).ToList(), startByteAddressDict["_Color"]);
    }

    private void AllocateInstanceDateBuffer()
    {
        //var count = BufferCountForInstances(kBytesPerInstance, kNumInstances, kExtraBytes); // 116
        var count = BRGTools.GetByteCount(
            //(typeof(Matrix4x4), 1), // 16 * 4 =64
            (typeof(float3x4), 3), //12*4*3 =144
            (typeof(float3x4), 3), // 12*4*3
            (typeof(float4), 3) //16*3
            ) / sizeof(int);

        Debug.Log($"count :{count}");
        instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, sizeof(int));
    }

    

    List<float3x4> objectToWorld;
    List<float3x4> worldToObject;
    List<Color> colors;

    // ---1
    List<int> startByteAddressList = new ();
    Dictionary<string, int> startByteAddressDict = new();

    Dictionary<string, (int startByteAddress, float[] drawDatas)> drawInfoDict = new();


    private void PopulateInstanceDataBuffer_float_span()
    {
        GenMaterialProperties();

        var metadataList = new NativeArray<MetadataValue>(3, Allocator.Temp);

        var drawInfos = new[]
        {
            objectToWorld.SelectMany(m => m.ToColumnArray()).ToArray(),
            worldToObject.SelectMany(m => m.ToColumnArray()).ToArray(),
            colors.SelectMany(v => v.ToArray()).ToArray(),
        };
        var matNames = new[]
        {
            "unity_ObjectToWorld",
            "unity_WorldToObject",
            "_Color"
        };

        var itemId = 0;
        for (int i = 0; i < drawInfos.Length; i++)
        {
            var datas = drawInfos[i];
            var matName = matNames[i];

            var startId = itemId;
            itemId += datas.Length;
            var endId = itemId;

            Debug.Log("startId : "+startId);


            instanceBuffer.SetData(datas, 0, startId, datas.Length);

            var startByteAddr = startId * GraphicsBufferTools.FLOAT_BYTES;
            // metadatas
            metadataList[i] = new MetadataValue
            {
                NameID = Shader.PropertyToID(matName),
                Value = (uint)(0x80000000 | startByteAddr)
            };

            startByteAddressDict.Add(matName, startByteAddr);
        }

        m_BatchID = BRGTools.AddBatch(ref m_BRG, metadataList, instanceBuffer);
        metadataList.Dispose();
    }


    private void PopulateInstanceDataBuffer_float_array()
    {
        GenMaterialProperties();

        var metadataList = new NativeList<MetadataValue>(3, Allocator.Temp);
        List<float> resultList = new();

        var drawInfos = new[]
        {
            (objectToWorld.SelectMany(m => m.ToColumnArray()).ToArray(), "unity_ObjectToWorld"),
            (worldToObject.SelectMany(m => m.ToColumnArray()).ToArray(), "unity_WorldToObject"),
            (colors.SelectMany(v => v.ToArray()).ToArray(), "_Color")
        };

        BRGTools.FillMatProperties(ref resultList, ref metadataList, ref drawInfoDict, drawInfos, false);

        instanceBuffer.SetData(resultList);

        m_BatchID = BRGTools.AddBatch(ref m_BRG, metadataList, instanceBuffer);
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

    private void PopulateInstanceDataBuffer_Vector4()
    {
        // Place a zero matrix at the start of the instance data buffer, so loads from address 0 return zero.
        var zero = new Matrix4x4[1] { Matrix4x4.zero };

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
        var colors = new List<Vector4>
        {
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1),
        };

        var metadataList = new NativeList<MetadataValue>(3, Allocator.Temp);
        List<Vector4> resultList = new();

        BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, zero.SelectMany(m => m.ToColumnVectors()), "");
        BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, objectToWorld.SelectMany(m => m.ToColumnVectors()), "unity_ObjectToWorld");
        BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, worldToObject.SelectMany(m => m.ToColumnVectors()), "unity_WorldToObject");
        BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, colors, "_Color");

        instanceBuffer.SetData(resultList);

        m_BatchID = BRGTools.AddBatch(ref m_BRG, metadataList, instanceBuffer);
        metadataList.Dispose();
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