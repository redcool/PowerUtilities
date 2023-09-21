using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PowerUtilities
{
    [ExecuteAlways]
    public class TMPTextEffects : MonoBehaviour
    {
        public enum MotionMode
        {
            Word,Character, Vertex
        }

        public TMP_Text tmpText;

        public MotionMode motionMode;

        [Header("Random")]
        public float speed = 1;
        public float amplitude = 1;

        [Header("Color")]
        public Gradient colorGradient;
        public float colorSpeed = 1;

        [Header("ReadOnly")]
        public List<int> wordIndices;

        // Start is called before the first frame update
        void OnEnable()
        {
            tmpText = GetComponent<TMP_Text>();

            if(!tmpText)
            {
                enabled = false;
                return;
            }

            wordIndices = GetWordIndices(tmpText.text);
        }

        // Update is called once per frame
        void Update()
        {
            tmpText.ForceMeshUpdate();

            var mesh = tmpText.mesh;
            switch (motionMode)
            {
                case MotionMode.Character:
                    UpdateMeshPerCharacter(mesh, tmpText, speed, amplitude);
                    break;
                case MotionMode.Vertex:
                    UpdateMeshPerVertex(mesh, speed, amplitude);
                    break;
                default:
                    UpdateMeshPerWord(mesh, tmpText, wordIndices, colorGradient, colorSpeed, speed, amplitude);
                    break;
            }
            
            tmpText.canvasRenderer.SetMesh(mesh);
        }

        public static void UpdateMeshPerVertex(Mesh mesh,float speed,float amplitude)
        {
            var vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++) 
            {
                Vector3 offset = Wobble(Time.time * speed + i, amplitude);
                vertices[i] += offset;
            }
            mesh.vertices = vertices;
        }

        public static void UpdateMeshPerCharacter(Mesh mesh,TMP_Text tmpText,float speed,float amplitude)
        {
            var vertices = mesh.vertices;
            for (int i = 0; i < tmpText.textInfo.characterCount; i++)
            {
                var info = tmpText.textInfo.characterInfo[i];
                var index = info.vertexIndex;
                Vector3 offset = Wobble(Time.time * speed + i, amplitude);
                vertices[index] += offset;
                vertices[index+1] += offset;
                vertices[index+2] += offset;
                vertices[index+3] += offset;
            }
            mesh.vertices = vertices;
        }

        public static void UpdateMeshPerWord(Mesh mesh,TMP_Text tmpText,List<int> wordIndices,Gradient colorGradient=default,float colorSpeed=1, float speed=1,float amplitude=1)
        {
            var text = tmpText.text;
            var vertices = mesh.vertices;
            var colors = mesh.colors;

            for (int i = 0; i < wordIndices.Count-1; i++)
            {
                var wordIndex = wordIndices[i];
                if (i > 0 && i < text.Length)
                    wordIndex++;

                var wordlength = wordIndices[i+1] - wordIndex;

                Vector3 offset = Wobble(Time.time * speed + wordIndex, amplitude);

                for (int j = 0; j < wordlength; j++)
                {
                    var chInfo = tmpText.textInfo.characterInfo[wordIndex + j];

                    var index = chInfo.vertexIndex;
                    vertices[index] += offset;
                    vertices[index+1] += offset;
                    vertices[index+2] += offset;
                    vertices[index+3] += offset;

                    if (colorGradient != null)
                    {
                        colors[index] = GetVertexColor(colorGradient, vertices[index].x * 0.001f, colorSpeed);
                        colors[index+1] = GetVertexColor(colorGradient, vertices[index+1].x * 0.001f, colorSpeed);
                        colors[index+2] = GetVertexColor(colorGradient, vertices[index+1].x * 0.001f, colorSpeed);
                        colors[index+3] = GetVertexColor(colorGradient, vertices[index+1].x * 0.001f, colorSpeed);
                    }
                }
            }

            mesh.vertices = vertices;
            mesh.colors = colors;
        }

        public static List<int> GetWordIndices(string text)
        {
            List<int> wordIndices = new List<int> { 0 };// first index
            wordIndices.AddRange(text.FindIndexAll(ch => ch == ' '));
            wordIndices.Add(text.Length); // last index
            return wordIndices;
        }

        public static Vector2 Wobble(float t,float amplitude=1)
        {
            return new Vector2(Mathf.Cos(t * 3.2f), MathF.Sin(t*2.3f)) * amplitude;
        }

        public static Color GetVertexColor(Gradient colorGradient, float offset,float speed)
        {
            //return Random.ColorHSV(0, offset);
            float t = Mathf.Repeat(Time.time * speed + offset * 0.001f, 1);
            t = Time.time % 1f;
            return colorGradient.Evaluate(t);
        }
    }
}
