using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

public class Test_Edge_Detection : MonoBehaviour
{
    public Texture inputTex;
    public RenderTexture outputTex;

    public ModelAsset modelAsset;

    [EditorButton(onClickCall = "Start")]
    public bool isStart;
    void Start()
    {
        outputTex = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat);

        var model = ModelLoader.Load(modelAsset);
        var inputTensor = new Tensor<float>(new TensorShape(1,3,256,256));
        TextureConverter.ToTensor(inputTex, inputTensor, new TextureTransform());

        var worker = new Worker(model, BackendType.GPUCompute);
        worker.SetInput("input", inputTensor);
        worker.Schedule();


        var outputTensor = worker.PeekOutput() as Tensor<float>;
        //outputTensor.Reshape(outputTensor.shape.Unsqueeze(0));
        TextureConverter.RenderToTexture(outputTensor,outputTex,new TextureTransform().SetCoordOrigin(CoordOrigin.TopLeft));
    }

}
