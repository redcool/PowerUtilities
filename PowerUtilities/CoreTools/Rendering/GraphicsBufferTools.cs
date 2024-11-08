using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class GraphicsBufferTools
    {
        public static int VECTOR4_BYTES = Marshal.SizeOf<Vector4>();
        public static int FLOAT_BYTES = Marshal.SizeOf<float>();
        /// <summary>
        /// update Vector4 data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="data"></param>
        /// <param name="startByteAddress"></param>
        public static void Update(this GraphicsBuffer buffer,List<Vector4> data, int startByteAddress)
        {
            buffer.SetData(data, 0, startByteAddress/ VECTOR4_BYTES, data.Count);
        }

        public static void Update(this GraphicsBuffer buffer, List<float> data, int startByteAddress)
        {
            buffer.SetData(data, 0, startByteAddress / FLOAT_BYTES, data.Count);
        }
    }



}
