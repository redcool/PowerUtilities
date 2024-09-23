namespace PowerUtilities.Net
{
    using System;
    [Serializable]

    public class MiniHttpKeyValuePair
    {
        public string key, value;
        public bool IsValid() => !string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(key);
    }
}
