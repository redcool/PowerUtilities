using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using PowerUtilities;

public class Test_TextureClassification : MonoBehaviour
{
    public ModelAsset modelAsset;

    [Header("Input")]
    public Texture2D inputTexture;

    [EditorButton(onClickCall = nameof(StartWork))]
    public bool isWork;

    public float[] results;


    public void SaveModel() { }
    public void StartWork()
    {
        var sourceModel = ModelLoader.Load(modelAsset);
        var graph = new FunctionalGraph();
        var inputs = graph.AddInputs(sourceModel);
        var outputs = Functional.Forward(sourceModel, inputs);
        var softmax = Functional.Softmax(inputs[0]);

        var runtimeModel = graph.Compile(softmax);
        //using var inputTensor = TextureConverter.ToTensor(inputTexture,width:28,height:28,channels:1);

        using var inputTensor = new Tensor<float>(new TensorShape(1, 1, 28, 28));
        TextureConverter.ToTensor(inputTexture, inputTensor, new TextureTransform());

        var worker = new Worker(runtimeModel,BackendType.CPU);
        worker.Schedule(inputTensor);

        var outputTensor = worker.PeekOutput() as Tensor<float>;
        results = outputTensor.DownloadToArray();

        worker.Dispose();
    }
}
