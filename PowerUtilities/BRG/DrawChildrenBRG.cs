#if UNITY_2022_2_OR_NEWER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    public class DrawChildrenBRG : MonoBehaviour
    {
        [EditorButton(onClickCall = "TestStart")]
        public bool isTest;

        public bool isIncludeInvisible = false;

        BatchRendererGroup brg;

        List<(BatchMeshID meshId, BatchMaterialID materialId)> batchList = new List<(BatchMeshID meshId, BatchMaterialID materialId)>();
        BatchID batchId;

        GraphicsBuffer instanceBuffer;

        const int 
            sizeOfMatrix = sizeof(float) * 4 * 4,
            sizeOfPackedMatrix = sizeof(float) * 4 * 3,
            sizeOfFloat4 = sizeof(float) * 4,
            bytesPerInstance = sizeOfPackedMatrix * 2 + sizeOfFloat4,
            extraBytes = sizeOfMatrix * 2;

        int
            numInstance = 3;

        struct PackedMatrix
        {
            float
                c0x, c0y, c0z,
                c1x, c1y, c1z,
                c2x, c2y, c2z,
                c3x, c3y, c3z;
            public PackedMatrix(Matrix4x4 m)
            {
                c0x = m.m00;
                c0y = m.m10;
                c0z = m.m20;
                c1x = m.m01;
                c1y = m.m11;
                c1z = m.m21;
                c2x = m.m02;
                c2y = m.m12;
                c2z = m.m22;
                c3x = m.m03;
                c3y = m.m13;
                c3z = m.m23;
            }
        }

        int BufferCountForInstances(int bytesPerInstance, int num, int extraBytes)
        {
            bytesPerInstance = (bytesPerInstance + sizeof(int) - 1) / sizeof(int) * sizeof(int);
            extraBytes = (extraBytes + sizeof(int) - 1) / sizeof(int) * sizeof(int);
            int totalBytes = bytesPerInstance * num + extraBytes;
            return totalBytes / sizeof(int);
        }

        void OnEnable()
        {
            if (brg == null)
                brg = new BatchRendererGroup(PerformCulling, IntPtr.Zero);

            RegisterChildren();

            instanceBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Raw,
                BufferCountForInstances(bytesPerInstance, numInstance, extraBytes),
                sizeof(int)
                );

            PopulateInstanceDataBuffer();
        }

        void PopulateInstanceDataBuffer()
        {
            var zero = new Matrix4x4[] { Matrix4x4.zero };

            var matrices = new Matrix4x4[]
            {
                Matrix4x4.Translate(new Vector3(-2,0,0)) ,
                Matrix4x4.Translate(new Vector3(0,0,0)),
                Matrix4x4.Translate(new Vector3(2,0,0))
            };

            var objectToWorld = new PackedMatrix[]
            {
                new PackedMatrix(matrices[0]),
                new PackedMatrix(matrices[1]),
                new PackedMatrix(matrices[2]),
            };

            var worldToObject = new PackedMatrix[]
            {
                new PackedMatrix(matrices[0].inverse),
                new PackedMatrix(matrices[1].inverse),
                new PackedMatrix(matrices[2].inverse),

            };

            var colors = new Vector4[]
            {
                new Vector4(1,0,0,1),
                new Vector4(0,1,0,1),
                new Vector4(0,0,1,1),
            };

            var byteAddressObjectToWorld = sizeOfPackedMatrix * 2;
            var byteAddressWorldToObject = sizeOfPackedMatrix * numInstance + byteAddressObjectToWorld;
            var byteAddressColor = byteAddressWorldToObject + sizeOfPackedMatrix * numInstance;

            instanceBuffer.SetData(zero, 0, 0, 1);
            instanceBuffer.SetData(objectToWorld, 0, byteAddressObjectToWorld / sizeOfPackedMatrix, objectToWorld.Length);
            instanceBuffer.SetData(worldToObject, 0, byteAddressWorldToObject / sizeOfPackedMatrix, worldToObject.Length);
            instanceBuffer.SetData(colors, 0, byteAddressColor / sizeOfFloat4, colors.Length);

            var metas = new MetadataValue[]
            {
                new MetadataValue{NameID = Shader.PropertyToID("unity_ObjectToWorld"),Value= (uint)byteAddressObjectToWorld},
                new MetadataValue{NameID = Shader.PropertyToID("unity_WorldToObject"),Value =(uint)byteAddressWorldToObject},
                new MetadataValue{NameID = Shader.PropertyToID("_BaseColor"),Value =(uint)byteAddressColor},
            };
            var metadata = new NativeArray<MetadataValue>(3, Allocator.Temp);
            metadata.CopyFrom(metas);

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES3)
            {
                batchId = brg.AddBatch(metadata, instanceBuffer.bufferHandle, 0, (uint)BatchRendererGroup.GetConstantBufferMaxWindowSize());
            }
            else
            {
                batchId = brg.AddBatch(metadata, instanceBuffer.bufferHandle);
            }
        }

        private void RegisterChildren()
        {
            var mrs = GetComponentsInChildren<MeshRenderer>(isIncludeInvisible);
            var infos = from mr in mrs
                        let mf = mr.GetComponent<MeshFilter>()
                        where mf is not null
                        select (
                            brg.RegisterMesh(mf.sharedMesh),
                            brg.RegisterMaterial(mr.sharedMaterial)
                        );

            batchList = infos.ToList();
        }

        private void OnDisable()
        {
            if(brg != null)
                brg.Dispose();
        }

        JobHandle PerformCulling(BatchRendererGroup brg,
            BatchCullingContext context,
            BatchCullingOutput output,
            IntPtr userContext
            )
        {
            return new JobHandle();
        }
    }
}
#endif