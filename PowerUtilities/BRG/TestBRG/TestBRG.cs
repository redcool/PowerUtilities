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
using UnityEngine.Assertions;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

[ExecuteInEditMode]
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
    
    public List<int> visibleIdList = new();

    //update
    public int updateId = 2;
    public Vector3[] offsets = new Vector3[3];
    public Color[] colorOffsets = new Color[3];

    [EditorButton(onClickCall = nameof(Start))]
    public bool isStart;

    // instance info
    List<float3x4> objectToWorlds;
    List<float3x4> worldToObjects;
    List<Color> colors;

    // {propName, startid = float count offset index}
    Dictionary<string, int> startIdDict = new();
    public float modelScale = 1;

    private void Start()
    {
        startIdDict.Clear();

        offsets = new Vector3[numInstances];
        colorOffsets = new Color[numInstances];
        for (int i = 0; i < numInstances; i++)
        {
            offsets[i] = Random.insideUnitSphere * 10;
            colorOffsets[i] = Random.ColorHSV();
        }


        if (m_BRG != null)
        {
            m_BRG.Dispose();
        }
        // register 
        m_BRG = new BatchRendererGroup(this.OnPerformCulling, IntPtr.Zero);
        m_MeshID = m_BRG.RegisterMesh(mesh);
        m_MaterialID = m_BRG.RegisterMaterial(material);

        //setup draw info
        NativeArray<MetadataValue> metadataArray = default;
        SetupMetaValues(ref metadataArray);

        GenMaterialProperties();
        SetupInstanceBuffer();


        m_BatchID = m_BRG.AddBatch(metadataArray, instanceBuffer);
        metadataArray.Dispose();
    }
    private void Update()
    {
        UpdateOneInstance(updateId);
        //UpdateOneInstance();
    }

    void UpdateOneInstance(int id)
    {
        var mat = Matrix4x4.Translate(offsets[id]);
        var objectToWorld = mat.ToFloat3x4();
        var worldToObject = mat.inverse.ToFloat3x4();
        var color = colorOffsets[id];

        objectToWorlds[0] = objectToWorld;
        worldToObjects[0] = worldToObject;
        colors[0] = color;

        instanceBuffer.SetData(objectToWorlds, 0, BRGTools.GetDataStartId(startIdDict,"unity_ObjectToWorld", 12, id,1), 1);
        instanceBuffer.SetData(worldToObjects, 0, BRGTools.GetDataStartId(startIdDict, "unity_WorldToObject", 12, id,1), 1);
        instanceBuffer.SetData(colors, 0, BRGTools.GetDataStartId(startIdDict, "_Color", 4, id,1), 1);
    }

    private void UpdateAllInstances()
    {
        var mats = new Matrix4x4[numInstances];
        for (int i = 0; i < numInstances; i++)
        {
            mats[i] = Matrix4x4.Translate(offsets[i]);
            objectToWorlds[i] = mats[i].ToFloat3x4();
            worldToObjects[i] = mats[i].inverse.ToFloat3x4();
            colors[i] = colorOffsets[i];
        }

        instanceBuffer.SetData(objectToWorlds, 0, BRGTools.GetDataStartId(startIdDict, "unity_ObjectToWorld", 12), objectToWorlds.Count);
        instanceBuffer.SetData(worldToObjects, 0, BRGTools.GetDataStartId(startIdDict, "unity_WorldToObject", 12), numInstances);
        instanceBuffer.SetData(colors, 0, BRGTools.GetDataStartId(startIdDict, "_Color", 4), numInstances);
    }



    private void SetupInstanceBuffer()
    {
        var count = BRGTools.GetByteCount(numInstances,
            typeof(float3x4),
            typeof(float3x4),
            typeof(float4));
        count /= 4;

        Assert.AreEqual(count, (12 + 12 + 4) * numInstances);

        Debug.Log($"all instance mat float count :{count} floats");
        if (!GraphicsBufferTools.IsValidSafe(instanceBuffer, GraphicsBuffer.Target.Raw, count, sizeof(int)))
        {
            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw, count, sizeof(int));
        }

        // fill buffer all instance data with zeros to avoid rendering garbage data before the first update
        instanceBuffer.SetData(objectToWorlds, 0, BRGTools.GetDataStartId(startIdDict, "unity_ObjectToWorld", 12), objectToWorlds.Count);
        instanceBuffer.SetData(worldToObjects, 0, BRGTools.GetDataStartId(startIdDict, "unity_WorldToObject", 12), numInstances);
        instanceBuffer.SetData(colors, 0, BRGTools.GetDataStartId(startIdDict, "_Color", 4), numInstances);
    }

    public void SetupMetaValues(ref NativeArray<MetadataValue> metadataArray)
    {
        var matPropInfos = new[]
        {
            ("unity_ObjectToWorld",12),
            ("unity_WorldToObject",12),
            ("_Color",4),
        };

        metadataArray = new NativeArray<MetadataValue>(matPropInfos.Length, Allocator.Temp);
        BRGTools.SetupMetadatas(numInstances, matPropInfos, ref metadataArray, startIdDict);

        Debug.Log("startIdDict : " + string.Join(',', startIdDict.Values));//0,12*instCount,24*instCount
    }

    private void GenMaterialProperties()
    {
        // Create transform matrices for three example instances.
        var matrices = new List<Matrix4x4>();
        colors = new List<Color>();
        for (int i = 0; i < numInstances; i++)
        {
            matrices.Add(Matrix4x4.TRS(transform.position + offsets[i],Quaternion.identity,Vector3.one * modelScale));
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
        // draw 
        var drawCmdPt = (BatchCullingOutputDrawCommands*)cullingOutput.drawCommands.GetUnsafePtr();
        BRGTools.SetupBatchDrawCommands(drawCmdPt, 1, numInstances);
        BRGTools.SetupBatchAllVisible(drawCmdPt, visibleIdList);

        BRGTools.SetupBatchDrawCommand(drawCmdPt, 0, m_BatchID, m_MaterialID, m_MeshID, visibleIdList.Count);


        return new JobHandle();
    }
}
#endif