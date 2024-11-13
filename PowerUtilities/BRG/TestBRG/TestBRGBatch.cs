#if UNITY_2022_2_OR_NEWER
using Codice.Client.BaseCommands;
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

    private BatchMeshID meshId;
    private BatchMaterialID matId;

    public int numInstances = 3;
    public int updateId = 2;
    public int updateMeshId = 0;
    //update
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    List<BRGBatch> brgBatches = new();

    private void Start()
    {
        offsets = new Vector3[numInstances];
        colorOffsets = new Color[numInstances];

        GenMaterialProperties();

        brg = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);


        matId = brg.RegisterMaterial(material);

        foreach (var mesh in meshes)
        {
            var meshId = brg.RegisterMesh(mesh);

            var brgBatch = new BRGBatch(brg, numInstances, meshId, matId);

            brgBatch.SetupGraphBuffer(12 + 12 + 4);

            for (int i = 0; i < numInstances; i++)
            {
                var objectToWorld = objectToWorlds[i];
                var worldToObject = worldToObjects[i];
                var color = colors[i];

                brgBatch.FillData(objectToWorld.ToColumnArray(), i, 0);
                brgBatch.FillData(worldToObject.ToColumnArray(), i, 1);
                brgBatch.FillData(color.ToArray(), i, 2);
            }
            brgBatches.Add(brgBatch);

        }
    }

    private void Update()
    {
        UpdateInst(updateId);
        //UpdateAll();
    }

    void UpdateInst(int instId)
    {
        var mat = Matrix4x4.Translate(offsets[instId]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[instId];

        var brgBatch = brgBatches[updateMeshId];
        {
            brgBatch.FillData(objectToWorld.ToColumnArray(), instId, 0);
            brgBatch.FillData(worldToObject.ToColumnArray(), instId, 1);
            brgBatch.FillData(color.ToArray(), instId, 2);
        }
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

        foreach (var brgBatch in brgBatches)
        {
            brgBatch.FillData(objectToWorlds.SelectMany(m => m.ToColumnArray()).ToArray(), 0, 0);
            brgBatch.FillData(worldToObjects.SelectMany(m => m.ToColumnArray()).ToArray(), 0, 1);
            brgBatch.FillData(colors.SelectMany(v => v.ToArray()).ToArray(), 0, 2);
        }
    }


    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    List<Color> colors;

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        var matrices = new List<Matrix4x4>();
        for (int i = 0;i < numInstances; i++)
        {
            matrices.Add(Matrix4x4.Translate(Vector3.Scale(Random.insideUnitSphere , Vector3.right) *2));
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
        foreach (var brgBatch in brgBatches)
        {
            brgBatch.DrawBatch(cullingOutput);
        }

        return new JobHandle();

    }
}
#endif