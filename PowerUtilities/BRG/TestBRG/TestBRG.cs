#if UNITY_2022_2_OR_NEWER
using PowerUtilities;
using PowerUtilities.CRP;
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
        var m = new float3x4();
        m.From(new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 });
        Debug.Log(m);

        var cols = m.ToColumnArray();
        Debug.Log(string.Join(',',cols));
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

        instanceBuffer.Update(objectToWorld.SelectMany(m => m.ToColumnVectors()).ToList(), startByteAddressList[0]);
        instanceBuffer.Update(worldToObject.SelectMany(m => m.ToColumnVectors()).ToList(), startByteAddressList[1]);
        instanceBuffer.Update(colors.SelectMany(v => v.ToArray()).ToList(), startByteAddressList[2]);
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

    private void PopulateInstanceDataBuffer_float()
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
        colors = new List<Color>
        {
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1),
        };

        var metadataList = new NativeList<MetadataValue>(3, Allocator.Temp);
        List<float> resultList = new();

        //BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, zero.SelectMany(m => m.ToColumnArray()), "");
        //BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, objectToWorld.SelectMany(m => m.ToColumnArray()), "unity_ObjectToWorld");
        //BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, worldToObject.SelectMany(m => m.ToColumnArray()), "unity_WorldToObject");
        //BRGTools.FillMatProperty(ref resultList, ref metadataList, ref startByteAddressList, colors.SelectMany(v=> v.ToArray()), "_Color");

        BRGTools.FillMatProperties(ref resultList, ref metadataList, ref startByteAddressList, new[]
        {
            (objectToWorld.SelectMany(m => m.ToColumnArray()), "unity_ObjectToWorld"),
            (worldToObject.SelectMany(m => m.ToColumnArray()), "unity_WorldToObject"),
            (colors.SelectMany(v => v.ToArray()), "_Color")
        });
        

        instanceBuffer.SetData(resultList);

        m_BatchID = BRGTools.AddBatch(ref m_BRG, metadataList, instanceBuffer);
        metadataList.Dispose();
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

        // UnsafeUtility.Malloc() requires an alignment, so use the largest integer type's alignment
        // which is a reasonable default.
        int alignment = UnsafeUtility.AlignOf<long>();

        // Acquire a pointer to the BatchCullingOutputDrawCommands struct so you can easily
        // modify it directly.
        var drawCommands = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();

        // Allocate memory for the output arrays. In a more complicated implementation, you would calculate
        // the amount of memory to allocate dynamically based on what is visible.
        // This example assumes that all of the instances are visible and thus allocates
        // memory for each of them. The necessary allocations are as follows:
        // - a single draw command (which draws kNumInstances instances)
        // - a single draw range (which covers our single draw command)
        // - kNumInstances visible instance indices.
        // You must always allocate the arrays using Allocator.TempJob.
        drawCommands->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>(), alignment, Allocator.TempJob);
        drawCommands->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);
        drawCommands->visibleInstances = (int*)UnsafeUtility.Malloc(numInstances * sizeof(int), alignment, Allocator.TempJob);
        drawCommands->drawCommandPickingInstanceIDs = null;

        drawCommands->drawCommandCount = 1;
        drawCommands->drawRangeCount = 1;
        drawCommands->visibleInstanceCount = numInstances;

        // This example doens't use depth sorting, so it leaves instanceSortingPositions as null.
        drawCommands->instanceSortingPositions = null;
        drawCommands->instanceSortingPositionFloatCount = 0;

        // Configure the single draw command to draw kNumInstances instances
        // starting from offset 0 in the array, using the batch, material and mesh
        // IDs registered in the Start() method. It doesn't set any special flags.
        drawCommands->drawCommands[0].visibleOffset = 0;
        drawCommands->drawCommands[0].visibleCount = (uint)numInstances;
        drawCommands->drawCommands[0].batchID = m_BatchID;
        drawCommands->drawCommands[0].materialID = m_MaterialID;
        drawCommands->drawCommands[0].meshID = m_MeshID;
        drawCommands->drawCommands[0].submeshIndex = 0;
        drawCommands->drawCommands[0].splitVisibilityMask = 0xff;
        drawCommands->drawCommands[0].flags = 0;
        drawCommands->drawCommands[0].sortingPosition = 0;

        // Configure the single draw range to cover the single draw command which
        // is at offset 0.
        drawCommands->drawRanges[0].drawCommandsBegin = 0;
        drawCommands->drawRanges[0].drawCommandsCount = 1;

        // This example doesn't care about shadows or motion vectors, so it leaves everything
        // at the default zero values, except the renderingLayerMask which it sets to all ones
        // so Unity renders the instances regardless of mask settings.
        drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };

        // Finally, write the actual visible instance indices to the array. In a more complicated
        // implementation, this output would depend on what is visible, but this example
        // assumes that everything is visible.
        for (int i = 0; i < numInstances; ++i)
            drawCommands->visibleInstances[i] = i;

        // This simple example doesn't use jobs, so it can just return an empty JobHandle.
        // Performance-sensitive applications should use Burst jobs to implement
        // culling and draw command output. In this case, this function would return a
        // handle here that completes when the Burst jobs finish.
        return new JobHandle();

    }
}
#endif