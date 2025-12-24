using UnityEngine;

public class CullingGroupRenderInstanced : MonoBehaviour
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

    RenderParams renderParams;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allMatrices = new Matrix4x4[totalInstances];
        spheres = new BoundingSphere[totalInstances];
        visibleMatrices = new Matrix4x4[totalInstances];

        visibleIndices = new int[totalInstances];
        block = new MaterialPropertyBlock();

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

        renderParams = new RenderParams(mat)
        {
            worldBounds = new Bounds(Vector3.zero, Vector3.one * 2000f),
            shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
            receiveShadows = true,
            matProps = block
        };
    }
 
    // Update is called once per frame
    void Update()
    {
        // all
        renderParams.matProps = null;
        Graphics.RenderMeshInstanced(renderParams, mesh, 0, allMatrices, totalInstances);

        if (!isShowVisible)
            return;

        renderParams.matProps = block;
        // visible only
        var visibleCount = cullingGroup.QueryIndices(true, visibleIndices, 0);
        if (visibleCount == 0)
            return;

        for (int i = 0; i < visibleCount; i++)
        {
            colors[i] = new Color(1, 0, 0, 1);
        }
        block.SetVectorArray("_BaseColor",colors);

        for (int i = 0; i < visibleCount; i++)
        {
            visibleMatrices[i] = allMatrices[visibleIndices[i]];
        }
        Graphics.RenderMeshInstanced(renderParams,mesh, 0, visibleMatrices, visibleCount);
    }
    private void OnDisable()
    {
        cullingGroup?.Dispose();
    }
}
