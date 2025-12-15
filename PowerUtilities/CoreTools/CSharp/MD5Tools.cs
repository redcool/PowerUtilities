using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PowerUtilities
{
    public static class MD5Tools
    {
        public static Lazy<MD5> lazyMD5 = new Lazy<MD5>(MD5.Create());
        
        public static string GetMD5HashString(this MD5 md5, Object unityObj)
        => md5.GetMD5HashString(JsonUtility.ToJson(unityObj));


        public static string GetMD5HashString(this MD5 md5, string objJson)
        {
            if(md5 == null)
                md5 = lazyMD5.Value;

            var bytes = Encoding.UTF8.GetBytes(objJson);
            var hashBytes = md5.ComputeHash(bytes);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            return sb.ToString();
        }

    }
}
