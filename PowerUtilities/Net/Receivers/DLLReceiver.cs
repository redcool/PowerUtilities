using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive *.dll
    /// </summary>
    public class DLLReceiver
    {

        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }

        private static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if(fileType.ToLower() == ".dll")
            {
                if(Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    Debug.LogError("IPhone do not support JIT");
                    return;
                }

                var asm = Assembly.LoadFile(filePath);
                if(asm == null)
                {
                    Debug.Log($"[DLLReceiver], dll not found,{filePath}");
                    return;
                }

                var defTypes = asm.DefinedTypes;
                foreach (var defType in defTypes)
                {
                    defType.InvokeMethod("Main", null, null, null);
                }

            }

        }
    }
}
