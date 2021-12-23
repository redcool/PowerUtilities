namespace PowerUtilities {
    using UnityEngine;
    using System.Collections;
    using UnityEngine.Networking;
    using System.Collections.Generic;
    using System;
    using Object = UnityEngine.Object;

    public class TestLoadBundle : MonoBehaviour {
        public string url;
        public bool startLoad;

        public string shaderName;

        public Dictionary<string, AssetBundle> bundleDict = new Dictionary<string, AssetBundle>();

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (startLoad)
            {
                startLoad = false;


                StartCoroutine(BundleLoader.WaitForLoadFromBoundle<Shader>(url, shaderName,shader=> {
                    GetComponent<Renderer>().material.shader = shader;
                }));
            }
        }

    }

}