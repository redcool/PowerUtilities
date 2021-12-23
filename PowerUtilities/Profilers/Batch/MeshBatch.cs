namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;

    public class MeshBatch : MonoBehaviour
    {
        public Material mat;
        // Use this for initialization
        void Start()
        {
            var rs = GetComponentsInChildren<MeshFilter>();
            var cs = new CombineInstance[rs.Length];

            for (int i = 0; i < rs.Length; i++)
            {
                cs[i].mesh = rs[i].sharedMesh;
                cs[i].transform = rs[i].transform.localToWorldMatrix;
                rs[i].gameObject.SetActive(false);
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(cs);
            gameObject.AddComponent<MeshFilter>().mesh = mesh;
            gameObject.AddComponent<MeshRenderer>().sharedMaterial = mat;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}