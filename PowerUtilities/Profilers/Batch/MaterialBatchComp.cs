namespace PowerUtilities
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    public class PropInfo
    {
        public int propId;
        public Type propType;
    }

    public class MaterialBatchComp : MonoBehaviour
    {
        public string shaderName = "Standard";


        Dictionary<Shader, List<Renderer>> shaderDict = new Dictionary<Shader, List<Renderer>>();
        Dictionary<string, List<PropInfo>> shaderPropDict = new Dictionary<string, List<PropInfo>>();
        Dictionary<Renderer, MaterialPropertyBlock> blockDict = new Dictionary<Renderer, MaterialPropertyBlock>();

        public Material mat;

        // Use this for initialization
        void Start()
        {
            SetupProperties();

            var rs = GetComponentsInChildren<MeshRenderer>();

            foreach (var r in rs)
            {
                r.enabled = false;
                var m = r.sharedMaterial;
                if (!m)
                    continue;

                if (!shaderDict.ContainsKey(m.shader))
                {
                    shaderDict.Add(m.shader, new List<Renderer>());
                }
                shaderDict[m.shader].Add(r);
            }
            ReplaceMaterials();
        }

        void SetupProperties()
        {
            var list = new List<PropInfo>();
            shaderPropDict.Add(shaderName, list);
            list.Add(new PropInfo { propId = Shader.PropertyToID("_NormTexture"), propType = typeof(Texture) });
            list.Add(new PropInfo { propId = Shader.PropertyToID("_NormScale"), propType = typeof(float) });
            list.Add(new PropInfo { propId = Shader.PropertyToID("_MainTex"), propType = typeof(Texture) });
        }

        void ReplaceMaterials()
        {
            //var matList = new List<Material>();
            foreach (var kv in shaderDict)
            {
                var shader = kv.Key;
                var list = kv.Value;

                //var mat = new Material(shader);

                foreach (var r in list)
                {
                    var lastMat = r.sharedMaterial;
                    //matList.Add(lastMat);

                    if (!blockDict.ContainsKey(r))
                        blockDict[r] = new MaterialPropertyBlock(); ;
                    var block = blockDict[r];

                    ReplaceMaterial(lastMat, block);

                    r.sharedMaterial = mat;
                    r.SetPropertyBlock(block);
                    r.enabled = true;
                }
            }

            //foreach (var mat in matList)
            //{
            //    Resources.UnloadAsset(mat);
            //}
        }

        void ReplaceMaterial(Material lastMat, MaterialPropertyBlock block)
        {
            Debug.Log("process " + lastMat + ":" + lastMat.shader.name);

            if (!shaderPropDict.ContainsKey(lastMat.shader.name))
            {
                return;
            }

            var list = shaderPropDict[lastMat.shader.name];
            foreach (var info in list)
            {
                SetValue(info, lastMat, block);
            }
        }

        void SetValue(PropInfo info, Material mat, MaterialPropertyBlock block)
        {
            if (info.propType == typeof(Texture))
            {
                var t = mat.GetTexture(info.propId);
                if (t)
                    block.SetTexture(info.propId, t);
            }
            else if (info.propType == typeof(float))
                block.SetFloat(info.propId, mat.GetFloat(info.propId));
        }
    }
}