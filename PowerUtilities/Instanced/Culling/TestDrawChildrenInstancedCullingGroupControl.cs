using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace PowerUtilities
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TestDrawChildrenInstancedCullingGroupControl))]

    public class TestCullingEditor : PowerEditor<TestDrawChildrenInstancedCullingGroupControl>
    {
        public override void DrawInspectorUI(TestDrawChildrenInstancedCullingGroupControl inst)
        {
            if (GUILayout.Button("add"))
            {

                DrawChildrenInstancedCullingGroupControl.SceneProfile.cullingInfos.Clear();
                var q = inst.list.Select(x => new CullingInfo(x.position));

                //DrawChildrenInstancedCullingGroupControl.SceneProfile.cullingInfos.AddRange(q);
            }
        }
    }
#endif

    public class TestDrawChildrenInstancedCullingGroupControl : MonoBehaviour
    {
        public List<Transform> list = new List<Transform>();

    }
}
