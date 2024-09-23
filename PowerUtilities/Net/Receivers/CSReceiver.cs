
using Slowsharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace PowerUtilities.Net
{
    /// <summary>
    /// Receive *.cs
    /// </summary>
    public class CSReceiver
    {
        [RuntimeInitializeOnLoadMethod]
        public static void Init()
        {
            MiniHttpServerComponent.OnFileReceived -= OnReceived;
            MiniHttpServerComponent.OnFileReceived += OnReceived;
        }

        public static void OnReceived(string fileName, string fileType, string filePath, List<MiniHttpKeyValuePair> headers)
        {
            if(fileType.ToLower().Contains(".cs"))
            {
                var csCode = File.ReadAllText(filePath);
                //CScriptUtils.RunScript(csCode);
            }
        }
    }
}
