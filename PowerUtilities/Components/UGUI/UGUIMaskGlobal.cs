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

        [Header("Update Renderer's material stencil")]
        public bool isUpdateRenderersMaterial;
        bool isUpdateRenderersMatLast;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        void Update()
        {
            if (CompareTools.CompareAndSet(ref isUpdateRenderersMatLast, ref isUpdateRenderersMaterial))
            {
                if (!isUpdateRenderersMaterial)
                    UpdateRenderersMaterial(CompareFunction.Disabled);
            }

        }


#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateStencilMat();
        }
#endif

        private void OnTransformChildrenChanged()
        {
            UpdateStencilMat();
        }

        private void OnDestroy()
        {
            if (uiMatStencilWriteInst)
                uiMatStencilReadInst.Destroy();

            if (uiMatStencilReadInst)
                uiMatStencilReadInst.Destroy();
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
        }

        public void UpdateStencilMat()
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

            var comp = stencilMat ? CompareFunction.Equal : CompareFunction.Disabled;
            if (isUpdateRenderersMaterial)
                UpdateRenderersMaterial(comp);
        }

        private void UpdateRenderersMaterial(CompareFunction comp)
        {
            var rs = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rs.Length; i++)
            {
                var r = rs[i];
                r.sharedMaterial.SetStencil(comp, stencilValue);
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
