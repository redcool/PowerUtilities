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
        var mats = new Matrix4x4[numInstances];
        for (int i = 0; i < numInstances; i++)
        {
            mats[i] = Matrix4x4.Translate(offsets[i]);
            objectToWorld[i] = mats[i].ToFloat3x4();
            worldToObject[i] = mats[i].inverse.ToFloat3x4();
            colors[i] = colorOffsets[i];
        }

        instanceBuffer.Update(objectToWorld.SelectMany(m => m.ToColumnVectors()).ToList(), startByteAddressDict["unity_ObjectToWorld"]);
        instanceBuffer.Update(worldToObject.SelectMany(m => m.ToColumnVectors()).ToList(), startByteAddressDict["unity_WorldToObject"]);
        instanceBuffer.Update(colors.SelectMany(v => v.ToArray()).ToList(), startByteAddressDict["_Color"]);
    }
    private void AllocateInstanceDateBuffer()
    {
        //var count = BufferCountForInstances(kBytesPerInstance, kNumInstances, kExtraBytes); // 116
        var count = BRGTools.GetByteCount(
            (typeof(Matrix4x4), 1) // 16 * 4 =64
            , (typeof(float3x4), 3) //12*4*3 =144
            , (typeof(float3x4), 3) // 12*4*3
            , (typeof(float4), 3) //16*3
            ) / sizeof(int);

        Debug.Log($"count :{count}");
        instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, sizeof(int));
    }

    
    List<int> startByteAddressList = new ();

    List<float3x4> objectToWorld;
    List<float3x4> worldToObject;
    List<Color> colors;
    Dictionary<string, int> startByteAddressDict = new();

    private void PopulateInstanceDataBuffer_float()
    {
        GenMaterialProperties();

        var metadataList = new NativeList<MetadataValue>(3, Allocator.Temp);
        List<float> resultList = new();

        BRGTools.FillMatProperties(ref resultList, ref metadataList, ref startByteAddressDict, new[]
        {
            (objectToWorld.SelectMany(m => m.ToColumnArray()), "unity_ObjectToWorld"),
            (worldToObject.SelectMany(m => m.ToColumnArray()), "unity_WorldToObject"),
            (colors.SelectMany(v => v.ToArray()), "_Color")
        }, false);


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