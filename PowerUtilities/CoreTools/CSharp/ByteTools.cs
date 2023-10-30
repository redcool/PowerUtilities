using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerUtilities
{
    public static unsafe class ByteTools
    {
        /// <summary>
        /// Reinterpret a buffer 
        /// get part of buffer's data
        /// 
        /// int idNum = 0;
        /// ByteTools.ReadBytes((byte*)&a, sizeof(int)+sizeof(BuiltinRenderTextureType), 4, (byte*)&idNum);
        /// 
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="results"></param>
        public static void ReadBytes(byte* buf, int start, int length, byte* results)
        {
            byte* p = buf + start;

            for (int i = 0; i < length; i++)
            {
                results[i] = *p;
                p++;
            }
        }

    }
}
