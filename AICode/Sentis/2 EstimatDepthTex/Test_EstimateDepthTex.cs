#if UNITY_SENTIS
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// download model: 
/// https://huggingface.co/unity/inference-engine-midas/tree/main/models
/// </summary>
public class Test_EstimateDepthTex : MonoBehaviour
{
    public ModelAsset modelAsset;
    public Texture2D inputTex;
    public RenderTexture outputTex;

    Model runtimeModel;
    Worker worker;

    public int size = 256;

    public int modelLayerCount;

    [EditorButton(onClickCall = "Start")]
    public bool isStart;

    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        var graph = new FunctionalGraph();
        var inputs = graph.AddInputs(runtimeModel);
        var outputs = Functional.Forward(runtimeModel, inputs);
        var output = outputs[0];

        var max0 = Functional.ReduceMax(output, new[] { 1, 2 }, false);
        var min0 = Functional.ReduceMin(output, new[] {1,2}, false);
        output = (output - min0)/(max0 - min0);
        runtimeModel = graph.Compile(output);
        modelLayerCount = runtimeModel.layers.Count;

        worker = new Worker(runtimeModel, BackendType.GPUCompute);

        outputTex = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);


        Run();
    }

    void Run()
    {
        using Tensor<float> inputTensor = new Tensor<float>(new TensorShape(1, 3, 256, 256));
        TextureConverter.ToTensor(inputTex, inputTensor, new TextureTransform());

        //worker.Schedule(inputTensor);

        var workQueue = worker.ScheduleIterable(inputTensor);
        var layerCount = runtimeModel.layers.Count;
        for (int i = 0; i < layerCount; i++)
        {
            if (!workQueue.MoveNext())
                break;
        }

        var outputTensor = worker.PeekOutput() as Tensor<float>;
        outputTensor.Reshape(outputTensor.shape.Unsqueeze(0));
        TextureConverter.RenderToTexture(outputTensor, outputTex, new TextureTransform().SetCoordOrigin(CoordOrigin.TopLeft));
    }
}
#endif