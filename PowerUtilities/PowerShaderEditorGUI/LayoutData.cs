using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PowerUtilities
{
    /// <summary>
    /// Layout.json structure
    /// </summary>
    /*
     
        LayoutData obj = new LayoutData();
        obj.contents = new[]{
            new JsonArrayWrapper<string>{array= new []{ "a","b"}}
        };
        obj.tabNames = new[] {"1","2" };

        var str = JsonUtility.ToJson(obj);
            
        var o = JsonUtility.FromJson<LayoutData>(str);
     */
    public class LayoutData
    {
        public string[] tabNames;

        public JsonArrayWrapper<string>[] contents;
    }


}
