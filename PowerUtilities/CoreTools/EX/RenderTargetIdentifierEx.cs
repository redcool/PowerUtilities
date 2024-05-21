using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowerUtilities
{
    /// <summary>
    /// open RenderTargetIdentifier fields
    /// </summary>
    public static class RenderTargetIdentifierEx
    {
        static FieldInfo 
            m_NameID,
            m_Type; //BuiltinRenderTextureType

        static RenderTargetIdentifierEx()
        {
            var t = typeof(RenderTargetIdentifier);
            m_NameID = t.GetField("m_NameID", BindingFlags.Instance | BindingFlags.NonPublic);
            m_Type = t.GetField("m_Type", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        static Dictionary<RenderTargetIdentifier, RTHandle> handleDict = new Dictionary<RenderTargetIdentifier, RTHandle>();

        /// <summary>
        /// Convert RenderTargetIdentifier to RTHandle with cache
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static RTHandle Convert(this RenderTargetIdentifier target)
        {
            return DictionaryTools.Get(handleDict, target, id => RTHandles.Alloc(id));
        }

        /// <summary>
        /// check nameId only
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool IsNameIdEquals(this RenderTargetIdentifier a, RenderTargetIdentifier b)
        {
            return GetNameId(a) == GetNameId(b);
        }

        /// <summary>
        /// RenderTargetIdentifier 's textureType
        /// </summary>
        /// <param name="rtId"></param>
        /// <returns></returns>
        public static BuiltinRenderTextureType GetBuiltinRenderTextureType(this RenderTargetIdentifier rtId)
        {
            unsafe
            {
                int idNum = 0;
                ByteTools.ReadBytes((byte*)&rtId, 0, sizeof(BuiltinRenderTextureType), (byte*)&idNum);
                return (BuiltinRenderTextureType)idNum;
            }
            //(BuiltinRenderTextureType)m_Type.GetValue(rtId);
        }

        /// <summary>
        /// Get nameId
        /// </summary>
        /// <param name="rtId"></param>
        /// <returns></returns>
        public static int GetNameId(this RenderTargetIdentifier rtId)
        {
            unsafe
            {
                int idNum = 0;
                ByteTools.ReadBytes((byte*)&rtId, sizeof(BuiltinRenderTextureType), sizeof(int), (byte*)&idNum);
                return idNum;
            }
            // 
            //var id = (int)m_NameID.GetValue(rtId);
            //return id;
        }

        /// <summary>
        /// Get instanceId
        /// </summary>
        /// <param name="rtId"></param>
        /// <returns></returns>
        public static int GetInstanceId(this RenderTargetIdentifier rtId)
        {
            unsafe
            {
                int idNum = 0;
                ByteTools.ReadBytes((byte*)&rtId, sizeof(int) + sizeof(BuiltinRenderTextureType), sizeof(int), (byte*)&idNum);
                return idNum;
            }
        }

    }
}
