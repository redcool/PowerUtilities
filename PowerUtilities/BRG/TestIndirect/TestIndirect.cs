#define INDEX_BUFFER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using Random = UnityEngine.Random;

public class TestIndirect : MonoBehaviour
{
    public Material material;
    public Mesh mesh;

    GraphicsBuffer meshTriangles;
    GraphicsBuffer meshPositions;
    GraphicsBuffer commandBuf;
#if INDEX_BUFFER
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
#else
    GraphicsBuffer.IndirectDrawArgs[] commandData;
#endif
    const int commandCount = 2;

    GraphicsBuffer posOffsetBuf;
    public float posOffsetY;
    GraphicsBuffer instanceCountBuf;
    [Range(0,1)]public float cullingRate = 0.6f;

    void Start()
    {
        // note: remember to check "Read/Write" on the mesh asset to get access to the geometry data
        meshTriangles = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
        meshTriangles.SetData(mesh.triangles);
        meshPositions = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, 3 * sizeof(float));
        meshPositions.SetData(mesh.vertices);
#if INDEX_BUFFER
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[commandCount];
#else
        commandBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, commandCount, GraphicsBuffer.IndirectDrawArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawArgs[commandCount];
#endif
    }

    void OnDestroy()
    {
        meshTriangles?.Dispose();
        meshTriangles = null;
        meshPositions?.Dispose();
        meshPositions = null;
        commandBuf?.Dispose();
        commandBuf = null;
    }

    void Update()
    {
        RenderParams rp = new RenderParams(material);
        rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one); // use tighter bounds
        rp.matProps = new MaterialPropertyBlock();
        rp.matProps.SetBuffer("_Triangles", meshTriangles);
        rp.matProps.SetBuffer("_Positions", meshPositions);
        rp.matProps.SetInt("_BaseVertexIndex", (int)mesh.GetBaseVertex(0));
        rp.matProps.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        rp.matProps.SetBuffer("_PosOffsets", posOffsetBuf);
        rp.matProps.SetBuffer("_InstanceCount", instanceCountBuf);
        rp.matProps.SetFloat("_IndexBufferOn", 0);

        var list = new List<Vector3>();
        var instanceCountList = new List<int>();
        SetupPosOffsetData(2, 10, list, instanceCountList);

        if (posOffsetBuf == null)
        {
            posOffsetBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 10 + 10, Marshal.SizeOf<Vector3>());
        }

        if (instanceCountBuf == null)
            instanceCountBuf = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 10 + 10, sizeof(int));

        posOffsetBuf.SetData(list);
        instanceCountBuf.SetData(instanceCountList);
#if INDEX_BUFFER
        rp.matProps.SetFloat("_IndexBufferOn",1);
        for (int i = 0; i < commandCount; i++)
        {
            commandData[i].indexCountPerInstance = mesh.GetIndexCount(0);
            commandData[i].instanceCount = (uint)instanceCountList[i];
            commandData[i].startIndex = mesh.GetIndexStart(0);
            commandData[i].baseVertexIndex = mesh.GetBaseVertex(0);
        }

        commandBuf.SetData(commandData);
        Graphics.RenderPrimitivesIndexedIndirect(rp, MeshTopology.Triangles, meshTriangles, commandBuf, commandCount);
#else
        for (int i = 0; i < commandCount; i++)
        {
            commandData[i].vertexCountPerInstance = mesh.GetIndexCount(0);
            commandData[i].startVertex = mesh.GetIndexStart(0);
            commandData[i].instanceCount = instanceCountList[i];
        }
        commandBuf.SetData(commandData);
        Graphics.RenderPrimitivesIndirect(rp, MeshTopology.Triangles, commandBuf, commandCount);
#endif
    }

    private List<Vector3> SetupPosOffsetData(int rows,int cols,List<Vector3> list,List<int> instanceCounts)
    {
        for (int i = 0; i < rows; i++) {
            int count = 0;
            for (int j = 0; j < cols; j++)
            {
                if (Random.value < cullingRate)
                    continue;

                list.Add(Vector3.right * j + Vector3.up * i * Random.Range(0,1f) * posOffsetY);
                count++;
            }

            instanceCounts.Add(count);
        }
        return list;
    }
}
