namespace PowerUtilities
{
    using System;
    using Object = UnityEngine.Object;

    /// <summary>
    /// unity asset info
    /// </summary>
    [Serializable]
    public class AssetFileInfo
    {
        public string guid;
        public string path;
        public Object asset;
        public long fileId;

        public void Clear()
        {
            path = "";
            fileId = 0;
            asset = null;
        }
    }
}
