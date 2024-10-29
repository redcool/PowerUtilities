using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    public class ImageNumberFX : MonoBehaviour
    {
        public enum RenderType
        {
            SpriteRenderer,UGUIImage
        }

        [HelpBox]
        public string help = "[0-9] number image for ugui";

        public RenderType renderType;

        public int num = 123;
        public List<Sprite> imageNums = new List<Sprite>();
        [Tooltip("SpriteRenderer 's Spacing")]
        public float spacing = 0;
        [EditorButton(onClickCall = "Sort")]
        public bool isSort;

        // ugui
        HorizontalLayoutGroup hGroup;

        int lastNum;


        // Start is called before the first frame update
        void OnEnable()
        {
            Sort();
        }

        // Update is called once per frame
        void Update()
        {
            if (lastNum != num || isSort)
            {
                lastNum = num;
                isSort = false;

                Sort();
            }
        }

        public void Sort()
        {
            gameObject.DestroyChildren();

            var numStr = num.ToString();
            for (int i = 0; i < numStr.Length; i++)
            {
                int id = numStr[i] - 48;

                if (id >= imageNums.Count)
                {
                    throw new Exception($"imageNums count not enough, count : {imageNums.Count},num id : {id}");
                }
                var sprite = imageNums[id];
                GameObject go = GenGO(id.ToString(),i, sprite);
            }

        }

        GameObject GenGO(string goName,int id, Sprite sprite)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform, false);

            SetupSprite(id,sprite, go);

            return go;
        }

        private void SetupSprite(int id, Sprite sprite, GameObject go)
        {
            if (renderType == RenderType.SpriteRenderer)
            {
                go.GetOrAddComponent<SpriteRenderer>().sprite = sprite;
                var posX = (sprite.bounds.size.x + spacing) * id;
                
                var pos = go.transform.position;
                pos.x = posX;
                go.transform.position = pos;
            }
            else
            {
                if (!hGroup)
                    hGroup = gameObject.GetOrAddComponent<HorizontalLayoutGroup>();

                go.GetOrAddComponent<Image>().sprite = sprite;
            }
        }

    }
}
