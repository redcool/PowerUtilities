#if UNITY_EDITOR
using UnityEditor;
using System.Linq;
#endif 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PowerUtilities;

[AddComponentMenu("WeatherComponents/ShowAttenField")]
public class ShowAttenField : MonoBehaviour {

}

#if UNITY_EDITOR
[CustomEditor(typeof(ShowAttenField))]
public class ShowAttenFieldEditor : Editor {

    class DrawInfo
    {
        public bool isDraw;
        public Color wireframeColor;
        public Material mat;
    }
    List<DrawInfo> infoList;
    const string ATTEN_FIELD = "_AttenField";
    Renderer r;
    void OnEnable()
    {
        var t = target as ShowAttenField;
        r = t.GetComponent<Renderer>();

        infoList = GetInfosFromSelection();
    }
    public override void OnInspectorGUI()
    {
        if(infoList != null)
            DrawInfoList(infoList);
    }

    void OnSceneGUI()
    {
        if (infoList == null)
            return;

        foreach (var item in infoList)
        {
            if (item.isDraw)
                DrawWireframe(item);
        }
    }

    List<DrawInfo> GetInfosFromSelection()
    {
        if (r)
        {
            var q = r.sharedMaterials
                .Where(mat => mat && mat.HasProperty(ATTEN_FIELD))
                .Select(mat => new DrawInfo
                {
                    isDraw = true,
                    wireframeColor = Random.ColorHSV(0f,1,0f,1,0.9f,1),
                    mat = mat
                });
            return q.ToList();
        }

        return null;
    }

    private void DrawInfoList(List<DrawInfo> infoList)
    {
        EditorGUILayout.BeginVertical("box");
        foreach (var item in infoList)
        {
            DrawUI(item);
        }
        EditorGUILayout.EndVertical();
    }

    private void DrawUI(DrawInfo item)
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.PrefixLabel(item.mat.name);
        EditorGUITools.DrawFixedWidthLabel(40, () =>
        {
            item.isDraw = EditorGUILayout.Toggle("绘制?", item.isDraw);
        });
        item.wireframeColor = EditorGUILayout.ColorField(item.wireframeColor);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawWireframe(DrawInfo item)
    {
        Handles.color = item.wireframeColor;
        Vector2 attenField = item.mat.GetVector(ATTEN_FIELD);
        Vector3 size = attenField;
        size.z = attenField.x;
        size = Vector3.Scale(size,r.transform.lossyScale);

        var startPos = r.transform.position;
        var centerPos = startPos + new Vector3(0, size.y,0);
        Handles.DrawWireCube(centerPos, size*2);
    }
}
#endif
