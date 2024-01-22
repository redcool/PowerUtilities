using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PowerUtilities
{
#if UNITY_EDITOR
using UnityEditor;
    [CustomEditor(typeof(TestShaderVariants))]
    public class TestShaderVariantsEditor : PowerEditor<TestShaderVariants>
    {
        public override void DrawInspectorUI(TestShaderVariants inst)
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Get keywords"))
            {
#if UNITY_2021_1_OR_NEWER
                inst.keywords = inst.shader.keywordSpace.keywordNames;
#else
                inst.keywords = new[] { "not collected in unity 2020" };
#endif
            }
            if(GUILayout.Button("Generate Cubes"))
            {
                inst.GenerateCubes();
            }
        }

    }
#endif

    public class TestShaderVariants : MonoBehaviour
    {
        public Shader shader;
        public string[] keywords;

        public int count = 100;
        public float distance = 5;

        public bool isAutoGenerate;
        // Start is called before the first frame update
        void Start()
        {
            if (isAutoGenerate)
                GenerateCubes();
        }

        public void GenerateCubes()
        {
            if (keywords == null || keywords.Length == 0)
                return;

            //var keywords = shader.keywordSpace.keywordNames;

            var mat = new Material(shader);

            for (int i = 0; i < count; i++)
            {
                var matInst = Instantiate(mat);
                matInst.shaderKeywords = keywords.Take(Random.Range(0, keywords.Length)).ToArray();

                var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubeGo.GetComponent<Renderer>().material = matInst;
                cubeGo.transform.SetParent(transform, false);
                cubeGo.transform.SetPositionAndRotation(transform.position + Random.insideUnitSphere * distance, transform.rotation);
            }
        }

    }
}
