#if UNITY_SENTIS
using PowerUtilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Sentis;
using UnityEngine;

public class Test_simple_add : MonoBehaviour
{
    public ModelAsset asset;

    [EditorButton(onClickCall = "Start")]
    public bool isTest;
    public void Start()
    {
        var model = ModelLoader.Load(asset);
        var worker = new Worker(model, BackendType.GPUCompute);

        Tensor<float> x = new Tensor<float>(new TensorShape(1, 3), new[] { 1f, 2, 3 });
        Tensor<float> y = new Tensor<float>(new TensorShape(1, 3), new[] { 10f, 20, 30f });
        worker.SetInput("input_x", x);
        worker.SetInput("input_y", y);
        worker.Schedule();

        var output = worker.PeekOutput() as Tensor<float>;
        var results = output.DownloadToArray();
        Debug.Log(string.Join(" , ", results));
    }
}
#endif