using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestTMP_Text : MonoBehaviour
{
    TMP_Text tmp_Text;
    Mesh mesh;

    List<int> wordIndexes = new List<int> { 0};
    List<int> wordLengths = new List<int>();
    public Gradient rainbow;
    // Start is called before the first frame update
    void Start()
    {
        tmp_Text = GetComponent<TMP_Text>();
        mesh = tmp_Text.mesh;

        var s = tmp_Text.text;
        for (int index = s.IndexOf(" "); index > -1; index = s.IndexOf(" ", index + 1))
        {
            wordLengths.Add(index - wordIndexes[wordIndexes.Count - 1]);
            wordIndexes.Add(index + 1);
        }
        wordLengths.Add(s.Length - wordIndexes[wordIndexes.Count-1]);
    }

    // Update is called once per frame
    void Update1()
    {
        tmp_Text.ForceMeshUpdate();

        mesh = tmp_Text.mesh;
        var vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 offset = Wobble(Time.time + i);
            vertices[i] = vertices[i] + offset;
        }
        //tmp_Text.SetVertices(vertices);
        mesh.vertices = vertices;
        tmp_Text.canvasRenderer.SetMesh(mesh);
    }

    private void Update2()
    {
        tmp_Text.ForceMeshUpdate();

        mesh = tmp_Text.mesh;
        var vertices = mesh.vertices;
        for (int i = 0; i < tmp_Text.textInfo.characterCount; i++)
        {
            var ch = tmp_Text.textInfo.characterInfo[i];
            var index = ch.vertexIndex;

            Vector3 offset = Wobble(Time.time + i);
            vertices[index] += offset;
            vertices[index + 1] += offset;
            vertices[index + 2] += offset;
            vertices[index + 3] += offset;
        }
        //tmp_Text.SetVertices(vertices);
        mesh.vertices = vertices;
        tmp_Text.canvasRenderer.SetMesh(mesh);
    }
    private void Update()
    {
        Debug.Log(Time.time + "," + Mathf.Repeat(Time.time, 3));

        tmp_Text.ForceMeshUpdate();

        mesh = tmp_Text.mesh;
        var vertices = mesh.vertices;
        var colors = mesh.colors;

        for (int w = 0; w < wordIndexes.Count; w++)
        {
            var wordIndex = wordIndexes[w];
            Vector3 offset = Wobble(Time.time + w);

            for (int i = 0; i < wordLengths[w]; i++)
            {
                var ch = tmp_Text.textInfo.characterInfo[i + wordIndex];
                var index = ch.vertexIndex;

                vertices[index] += offset;
                vertices[index + 1] += offset;
                vertices[index + 2] += offset;
                vertices[index + 3] += offset;

                colors[index] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index].x * 0.001f, 1f));
                colors[index+1] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index+1].x * 0.001f, 1f));
                colors[index+2] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index+2].x * 0.001f, 1f));
                colors[index+3] = rainbow.Evaluate(Mathf.Repeat(Time.time + vertices[index+3].x * 0.001f, 1f));
            }
        }
        //tmp_Text.SetVertices(vertices);
        mesh.vertices = vertices;
        mesh.colors = colors;
        tmp_Text.canvasRenderer.SetMesh(mesh);
    }
    private Vector2 Wobble(float v)
    {
        return new Vector2(Mathf.Sin(v * 3.2f), MathF.Cos(v * 2.3f));
    }
}
