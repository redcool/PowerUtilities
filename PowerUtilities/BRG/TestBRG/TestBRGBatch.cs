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
using Random = UnityEngine.Random;


public class TestBRGBatch : MonoBehaviour
{
    [EditorButton(onClickCall = "OnTest")]
    public bool isTest;

    void OnTest()
    {

    }

    //public Mesh mesh;
    public Material material;
    public Mesh[] meshes;

    private BatchRendererGroup brg;

    public int numInstancesPerGroup = 3;
    int numInstances;

    public int updateId = 2;
    public int updateMeshId = 0;
    //update
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    List<BRGBatch> brgBatches = new();

    private void Start()
    {
        numInstances = numInstancesPerGroup * meshes.Length;

        offsets = new Vector3[numInstances];
        colorOffsets = new Color[numInstances];

        GenMaterialProperties();

        brg = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);


        var matId = brg.RegisterMaterial(material);

        for (int j = 0; j < meshes.Length; j++)
        {
            var mesh = meshes[j];
            var meshId = brg.RegisterMesh(mesh);

            var brgBatch = new BRGBatch(brg, numInstancesPerGroup, meshId, matId,j);

            brgBatch.Setup(12 + 12 + 4,
                new[] {"unity_ObjectToWorld","unity_WorldToObject","_Color" } ,
                new List<int>{ 12,12,4}
                );

            for (int i = 0; i < numInstancesPerGroup; i++)
            {
                var objectToWorld = objectToWorlds[i + j * numInstancesPerGroup];
                var worldToObject = worldToObjects[i + j * numInstancesPerGroup];
                var color = colors[i + j * numInstancesPerGroup];

                brgBatch.FillData(objectToWorld.ToColumnArray(), i, 0);
                brgBatch.FillData(worldToObject.ToColumnArray(), i, 1);
                brgBatch.FillData(color.ToArray(), i, 2);
            }
            brgBatches.Add(brgBatch);

        }
    }

    private void Update()
    {
        UpdateInst(updateId, updateMeshId);
        //UpdateAll();
    }

    void UpdateInst(int instId,int batchId)
    {
        var instIdGlobal = instId + batchId * numInstancesPerGroup;
        var mat = Matrix4x4.Translate(offsets[instIdGlobal]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[instIdGlobal];

        var brgBatch = brgBatches[batchId];
        {
            brgBatch.FillData(objectToWorld.ToColumnArray(), instId, 0);
            brgBatch.FillData(worldToObject.ToColumnArray(), instId, 1);
            brgBatch.FillData(color.ToArray(), instId, 2);
        }
    }

    private void UpdateAll()
    {
        var id = 0;
        for (int j = 0; j < meshes.Length; j++)
        {

            for (int i = 0; i < numInstancesPerGroup; i++)
            {
                matrices[id] = Matrix4x4.Translate(offsets[id] + Vector3.right * i + Vector3.up * j);
                objectToWorlds[id] = matrices[id].ToFloat3x4();
                worldToObjects[id] = matrices[id].inverse.ToFloat3x4();
                colors[id] = colorOffsets[id];

                id++;
            }
        }

        for (int i = 0; i < brgBatches.Count; i++)
        {
            var brgBatch = brgBatches[i];
            
            brgBatch.FillData(objectToWorlds.Skip(i * numInstancesPerGroup).Take(numInstancesPerGroup).SelectMany(m => m.ToColumnArray()).ToArray(), 0, 0);
            brgBatch.FillData(worldToObjects.Skip(i * numInstancesPerGroup).Take(numInstancesPerGroup).SelectMany(m => m.ToColumnArray()).ToArray(), 0, 1);
            brgBatch.FillData(colors.Skip(i * numInstancesPerGroup).Take(numInstancesPerGroup).SelectMany(v => v.ToArray()).ToArray(), 0, 2);
        }
    }


    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    List<Color> colors;

    List<Matrix4x4> matrices;

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        matrices = new List<Matrix4x4>();

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

        var numInstances = brgBatches.Sum(b=> b.numInstances);
        BRGTools.SetupBatchDrawCommands(drawCmdPt, brgBatches.Count, numInstances);
        foreach (var batch in brgBatches)
        {
            batch.DrawBatch(drawCmdPt);
        }

        return new JobHandle();
    }

    public unsafe JobHandle OnPerformCulling_TestOK(
        BatchRendererGroup rendererGroup,
        BatchCullingContext cullingContext,
        BatchCullingOutput cullingOutput,
        IntPtr userContext)
    {
        var numInstances = brgBatches.Sum(b=>b.numInstances);

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
        drawCommands->drawCommands = (BatchDrawCommand*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawCommand>()*brgBatches.Count, alignment, Allocator.TempJob);
        drawCommands->drawRanges = (BatchDrawRange*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<BatchDrawRange>(), alignment, Allocator.TempJob);
        drawCommands->visibleInstances = (int*)UnsafeUtility.Malloc(numInstances * sizeof(int), alignment, Allocator.TempJob);
        drawCommands->drawCommandPickingInstanceIDs = null;

        drawCommands->drawCommandCount = brgBatches.Count;
        drawCommands->drawRangeCount = 1;
        drawCommands->visibleInstanceCount = numInstances;

        // This example doens't use depth sorting, so it leaves instanceSortingPositions as null.
        drawCommands->instanceSortingPositions = null;
        drawCommands->instanceSortingPositionFloatCount = 0;

        // Configure the single draw command to draw kNumInstances instances
        // starting from offset 0 in the array, using the batch, material and mesh
        // IDs registered in the Start() method. It doesn't set any special flags.
        drawCommands->drawCommands[0].batchID = brgBatches[0].batchId;
        drawCommands->drawCommands[0].flags = 0;
        drawCommands->drawCommands[0].materialID = brgBatches[0].matId;
        drawCommands->drawCommands[0].meshID = brgBatches[0].meshId;
        drawCommands->drawCommands[0].sortingPosition = 0;
        drawCommands->drawCommands[0].splitVisibilityMask = 0;
        drawCommands->drawCommands[0].submeshIndex = 0;
        drawCommands->drawCommands[0].visibleCount = (uint)brgBatches[0].numInstances;
        drawCommands->drawCommands[0].visibleOffset = 0;

        drawCommands->drawCommands[1].batchID = brgBatches[1].batchId;
        drawCommands->drawCommands[1].flags = 0;
        drawCommands->drawCommands[1].materialID = brgBatches[1].matId;
        drawCommands->drawCommands[1].meshID = brgBatches[1].meshId;
        drawCommands->drawCommands[1].sortingPosition = 0;
        drawCommands->drawCommands[1].splitVisibilityMask = 0;
        drawCommands->drawCommands[1].submeshIndex = 0;
        drawCommands->drawCommands[1].visibleCount = (uint)brgBatches[1].numInstances;
        drawCommands->drawCommands[1].visibleOffset = 0;

        // Configure the single draw range to cover the single draw command which
        // is at offset 0.
        drawCommands->drawRanges[0].drawCommandsBegin = 0;
        drawCommands->drawRanges[0].drawCommandsCount = (uint)drawCommands->drawCommandCount;
        drawCommands->drawRanges[0].filterSettings = new BatchFilterSettings { renderingLayerMask = 0xffffffff, };


        // Finally, write the actual visible instance indices to the array. In a more complicated
        // implementation, this output would depend on what is visible, but this example
        // assumes that everything is visible.
        for (int i = 0; i < numInstances; ++i)
            drawCommands->visibleInstances[i] = i;

        //foreach (var brgBatch in brgBatches)
        //{
        //    brgBatch.DrawBatch(cullingOutput);
        //}

        return new JobHandle();

    }
}
#endif