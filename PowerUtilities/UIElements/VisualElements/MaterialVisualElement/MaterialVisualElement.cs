using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Mesh;
using Object = UnityEngine.Object;

public class UxmlObjectAttributeDescription : TypedUxmlAttributeDescription<Object>
{
    public override Object GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
    {
        MethodInfo method = typeof(VisualTreeAsset).GetMethod("GetUxmlObjects",
    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (method != null)
        {
            var generic = method.MakeGenericMethod(typeof(Object));
            var list = generic.Invoke(cc.visualTreeAsset, new object[] { bag, cc }) as List<Object>;
            if (list != null && list.Count > 0) return list[0];
        }
        //    MethodInfo getUxmlObjectsMethod = typeof(VisualTreeAsset).GetMethod("GetUxmlObjects",
        //BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        //    if (getUxmlObjectsMethod != null)
        //    {
        //        var genericMethod = getUxmlObjectsMethod.MakeGenericMethod(typeof(Object));
        //        var results = genericMethod.Invoke(cc.visualTreeAsset, new object[] { bag, cc }) as List<Object>;
        //        return results[0];
        //    }
        return null;
    }
}

public class MaterialVisualElement : VisualElement
{
    public Material customMaterial;

    public object _noneMeshFlags;
    public MethodInfo _allocateMethod;
    public MaterialVisualElement()
    {
        generateVisualContent += OnGen;
    }

    public MethodInfo GetAllocate()
    {
        // 1. 获取内部枚举 MeshFlags 的类型
        Type meshFlagsType = typeof(MeshGenerationContext).GetNestedType("MeshFlags", BindingFlags.NonPublic);

        // 2. 获取 MeshFlags.None 的值
        _noneMeshFlags = Enum.ToObject(meshFlagsType, 0);

        if (_allocateMethod == null)
        {
            // 寻找匹配的私有/内部 Allocate 方法
            _allocateMethod = typeof(MeshGenerationContext).GetMethod("Allocate",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                new[] { typeof(int), typeof(int), typeof(Texture), typeof(Material), meshFlagsType },
                null);
        }
        return _allocateMethod;
    }

    void OnGen(MeshGenerationContext mgc)
    {
        if (customMaterial == null) return;

        Rect rect = contentRect;
        if (rect.width <= 0 || rect.height <= 0) return;

        // 分配 4 个顶点和 6 个索引（构成两个三角形，即一个矩形）
        // 第三个参数传入你的自定义材质
        var allocateMethod = GetAllocate();

        var meshData = (MeshWriteData)allocateMethod.Invoke(mgc, new object[] { 4, 6, null, customMaterial, _noneMeshFlags });

        // --- 填充几何体 (标准四边形) ---
        float left = rect.xMin;
        float right = rect.xMax;
        float top = rect.yMin;
        float bottom = rect.yMax;

        Vertex[] vertices = new Vertex[4];
        vertices[0] = new Vertex { position = new Vector3(left, top, Vertex.nearZ), uv = new Vector2(0, 1), tint = Color.white };
        vertices[1] = new Vertex { position = new Vector3(right, top, Vertex.nearZ), uv = new Vector2(1, 1), tint = Color.white };
        vertices[2] = new Vertex { position = new Vector3(right, bottom, Vertex.nearZ), uv = new Vector2(1, 0), tint = Color.white };
        vertices[3] = new Vertex { position = new Vector3(left, bottom, Vertex.nearZ), uv = new Vector2(0, 0), tint = Color.white };

        meshData.SetAllVertices(vertices);
        meshData.SetAllIndices(new ushort[] { 0, 1, 2, 2, 3, 0 });
    }

}
// 为了让它能在 UI Builder 的 Library 中显示，需要定义一个 Factory
public  class UxmlFactory : UxmlFactory<MaterialVisualElement, UxmlTraits> { }

public class UxmlTraits : VisualElement.UxmlTraits
{
    UxmlObjectAttributeDescription materialDesc = new()
    {
        name = "Custom material",
        defaultValue = null,
    };
    public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
    {
        base.Init(ve, bag, cc);
        var element = (MaterialVisualElement)ve;

        element.customMaterial = (Material)materialDesc.GetValueFromBag(bag, cc);
    }


}
