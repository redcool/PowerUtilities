using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TestCulling))]

    public class TestCullingEditor : PowerEditor<TestCulling>
    {
        public override void DrawInspectorUI(TestCulling inst)
        {
            if (GUILayout.Button("add"))
            {

                CullingGroupControl.SceneProfile.cullingInfos.Clear();
                var q = inst.list.Select(x => new CullingInfo(x.position));

                CullingGroupControl.SceneProfile.cullingInfos.AddRange(q);
            }
        }
    }
#endif

    public class TestCulling : MonoBehaviour
    {
        public List<Transform> list = new List<Transform>();

    }
}
