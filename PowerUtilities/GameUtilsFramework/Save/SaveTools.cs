using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameUtilsFramework
{
    public static class SaveTools
    {
        public static string ObjectToJson(object obj)
        {
            var json = JsonUtility.ToJson(obj);
            return json;
        }

        public static void SaveObject(object obj,string filePath)
        {
            var str = ObjectToJson(obj).ToBase64().Encrypt();
            File.WriteAllText(filePath, str);
        }

        public static T JsonToObject<T>(string json)
        {
            var obj = JsonUtility.FromJson<T>(json);
            return obj;
        }

        public static T ReadObject<T>(string filePath)
        {
            var str = File.ReadAllText(filePath).Decrypt().FromBase64();
            return JsonToObject<T>(str);
        }
    }
}
