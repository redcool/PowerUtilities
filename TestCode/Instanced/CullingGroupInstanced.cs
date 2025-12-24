using UnityEngine;

public class CullingGroupInstanced : MonoBehaviour
{
    public Mesh mesh;
    public Material mat;

    public int totalInstances = 10000;
    Matrix4x4[] allMatrices, visibleMatrices;

    BoundingSphere[] spheres;

    CullingGroup cullingGroup;
    int [] visibleIndices;

    MaterialPropertyBlock block;
    Vector4[] colors;
    public bool isShowVisible;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allMatrices = new Matrix4x4[totalInstances];
        spheres = new BoundingSphere[totalInstances];
        visibleMatrices = new Matrix4x4[totalInstances];

        visibleIndices = new int[totalInstances];

        colors = new Vector4[totalInstances];

        for (int i = 0; i < totalInstances; i++)
        {
            Vector3 position = new Vector3(
                Random.Range(-500f, 500f),
                Random.Range(-500f, 500f),
                Random.Range(-500f, 500f)
            );
            allMatrices[i] = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one*600);
            spheres[i] = new BoundingSphere(position, 2f);
        }

        cullingGroup = new CullingGroup();
        cullingGroup.SetBoundingSpheres(spheres);
        cullingGroup.SetBoundingSphereCount(totalInstances);
        cullingGroup.targetCamera = Camera.main;
    }
 
    // Update is called once per frame
    void Update()
    {
        // all
        Graphics.DrawMeshInstanced(mesh, 0, mat, allMatrices, totalInstances);

        if (!isShowVisible)
            return;
        // visible only
        var visibleCount = cullingGroup.QueryIndices(true, visibleIndices, 0);
        if (visibleCount == 0)
            return;

        if(block == null)
            block = new MaterialPropertyBlock();

        for (int i = 0; i < visibleCount; i++)
        {
            colors[i] = new Color(1, 0, 0, 1);
        }
        block.SetVectorArray("_BaseColor",colors);

        for (int i = 0; i < visibleCount; i++)
        {
            visibleMatrices[i] = allMatrices[visibleIndices[i]];
        }
        Graphics.DrawMeshInstanced(mesh, 0, mat, visibleMatrices, visibleCount,block);
    }
    private void OnDisable()
    {
        cullingGroup?.Dispose();
    }
}
