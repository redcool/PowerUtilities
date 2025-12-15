namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;


    /// <summary>
    /// JsonUtility support 1 dimension array only, use this wrapper multi dimension array
    /// 
    /*

    // class    
        public class LayoutData
        {
            public string[] tabNames;

            public JsonArrayWrapper<string>[] contents;
        }

    // serialize
            if (GUILayout.Button("Test"))
            {
                LayoutData obj = new LayoutData();
                obj.contents = new[]{
                    new JsonArrayWrapper<string>{array= new []{ "a","b"}}
                };
                obj.tabNames = new[] {"1","2" };

                var str = JsonUtility.ToJson(obj);

                var o = JsonUtility.FromJson<LayoutData>(str);

            }
    */
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public class JsonArrayWrapper<T>
    {
        public T[] array;
    }
}