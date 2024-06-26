namespace PowerUtilities
{
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using UnityEngine.UI;

#if UNITY_EDITOR

    [CustomEditor(typeof(UGUIMaskGlobal))]
    public class UGUIMaskGlobalEditor : PowerEditor<UGUIMaskGlobal>
    {
        public override string Version => "0.0.1";
        
        public override string TitleHelpStr => "Draw UI with stencil control";

        public override bool NeedDrawDefaultUI() => true;
        


    }
#endif

    [ExecuteAlways]
    public class UGUIMaskGlobal : MonoBehaviour
    {
        [Header("Stencil Write")]
        [LoadAsset("UI_Default Stencil Write.mat")]
        public Material uiMatStencilWrite;
        Material uiMatStencilWriteInst;

        [Range(0,255)]public int stencilValue = 1;
        public bool isShowMaskGraphic;
        public bool isUseUIAlphaClip = true;

        [Header("Stencil Read")]
        [LoadAsset("UI_Default Stencil Read.mat")]
        public Material uiMatStencilRead;
        Material uiMatStencilReadInst;
        
        public Graphic graphic;

        [Header("Update Partcile")]
        public bool isUpdateParticleMaterial;
        bool isUpdateParticleMaterialLast;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        void Update()
        {
            if(isUpdateParticleMaterialLast != isUpdateParticleMaterial)
            {
                if (!isUpdateParticleMaterial)
                    UpdateParticleMaterial(CompareFunction.Disabled);

                isUpdateParticleMaterialLast = isUpdateParticleMaterial;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateStencilMat();
            Debug.Log("validate");
        }
#endif

        private void OnTransformChildrenChanged()
        {
            UpdateStencilMat();
            Debug.Log("children changed");
        }

        private void OnDestroy()
        {
            if (uiMatStencilWriteInst)
                uiMatStencilReadInst.Destroy();

            if (uiMatStencilReadInst)
                uiMatStencilReadInst.Destroy();
        }

        private void UpdateStencilMat()
        {
            if (!uiMatStencilWriteInst || !uiMatStencilReadInst)
                return;

            uiMatStencilWriteInst.SetFloat("_Stencil", stencilValue);
            uiMatStencilWriteInst.SetFloat("_ColorMask", isShowMaskGraphic ? 15 : 0);
            uiMatStencilWriteInst.SetFloat("_UseUIAlphaClip", isUseUIAlphaClip ? 1 : 0);
            uiMatStencilWriteInst.SetKeyword("UNITY_UI_ALPHACLIP", isUseUIAlphaClip);

            uiMatStencilReadInst.SetFloat("_Stencil", stencilValue);

            UpdateChildrenMat(uiMatStencilReadInst);
        }

        private void UpdateChildrenMat(Material stencilMat)
        {
            UpdateUIMaterial(stencilMat);

            if(isUpdateParticleMaterial)
                UpdateParticleMaterial(CompareFunction.Equal);
        }

        private void UpdateParticleMaterial(CompareFunction comp)
        {

            var ps = GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < ps.Length; i++)
            {
                var p = ps[i];
                var pr = p.GetComponent<ParticleSystemRenderer>();
                var mat = pr.sharedMaterial;
                if (mat.HasProperty("_Stencil"))
                    mat.SetFloat("_Stencil", stencilValue);
                if (mat.HasProperty("_StencilComp"))
                    mat.SetFloat("_StencilComp",(int)comp);
            }
        }

        private void UpdateUIMaterial(Material stencilMat)
        {
            var items = GetComponentsInChildren<Graphic>();
            for (int i = 1; i < items.Length; i++)
            {
                var item = items[i];

                item.material = stencilMat;
            }
        }

        private void OnEnable()
        {
            SetupStencilMaterials();

            graphic = GetComponent<Graphic>();
            if (!graphic)
                graphic = gameObject.AddComponent<RawImage>();

            graphic.material = uiMatStencilWriteInst;

            UpdateStencilMat();
        }

        private void OnDisable()
        {
            UpdateChildrenMat(null);

            if (isUpdateParticleMaterial)
                UpdateParticleMaterial(CompareFunction.Disabled);
        }


        private void SetupStencilMaterials()
        {
            if (!uiMatStencilWrite)
                uiMatStencilWrite = Resources.Load<Material>("UI_Default Stencil Write");

            if (!uiMatStencilWriteInst)
            {
                uiMatStencilWriteInst = Instantiate(uiMatStencilWrite);// new Material(uiMatStencilWrite);
                uiMatStencilWriteInst.name = "uiMatStencilWriteInst";
            }

            if (!uiMatStencilRead)
                uiMatStencilRead = Resources.Load<Material>("UI_Default Stencil Read");
            if (!uiMatStencilReadInst)
            {
                uiMatStencilReadInst = Instantiate(uiMatStencilRead); // new Material(uiMatStencilRead);
                uiMatStencilReadInst.name = "uiMatStencilReadInst";
            }
        }



    }
}
