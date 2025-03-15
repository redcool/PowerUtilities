namespace PowerUtilities {
    using UnityEngine;
    using System.Collections;
    using UnityEngine.Networking;
    using System.Collections.Generic;
    using System;
    using Object = UnityEngine.Object;

    public class TestLoadBundle : MonoBehaviour {
        public string url;
        [Header("test load shader bundle")]
        public bool startLoadShader;
        public string shaderName;

        [Header("Test load object")]
        public bool startLoadObject;
        public string objectName;

        public bool isStartLoadManifest;

        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {
            if (startLoadShader)
            {
                startLoadShader = false;


                StartCoroutine(BundleLoader.WaitForLoadFromBundle<Shader>(url, shaderName,shader=> {
                    GetComponent<Renderer>().material.shader = shader;
                }));
            }

            if(startLoadObject)
            {
                startLoadObject = false;
                StartCoroutine(BundleLoader.WaitForLoadFromBundle<GameObject>(url, objectName, obj =>
                {
                    Instantiate(obj);
                }));
            }

            if(isStartLoadManifest)
            {
                isStartLoadManifest = false;
                StartCoroutine(BundleLoader.WaitForLoadManifestFromBundle(url, mani =>
                {
                    foreach (var abName in mani.GetAllAssetBundles())
                    {
                        Debug.Log(abName +" -> "+string.Join(",", mani.GetAllDependencies(abName)));
                    }

                }));
            }
        }

    }

}