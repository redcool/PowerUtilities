namespace PowerUtilities.RenderFeatures
{
    using System;

    [Serializable]
    public class ShaderValue<T> 
    {
        public string name;
        public T value;

        public bool IsValid => !string.IsNullOrEmpty(name) && (typeof(T).IsClass ? value != null : true);
    }
}
