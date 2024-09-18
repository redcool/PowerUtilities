using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PowerUtilities
{
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class UGUIImageNumber : MonoBehaviour
    {
        public int num = 123;
        public List<Sprite> imageNums = new List<Sprite>();

        [EditorButton(onClickCall ="Sort")]
        public bool isSort;


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
            if(lastNum != num || isSort)
            {
                lastNum = num;
                isSort = false;

                Sort();
            }
        }

        private void Sort()
        {
            if (!hGroup)
                hGroup = gameObject.GetComponent<HorizontalLayoutGroup>();

            if(hGroup.transform.childCount > 0)
                hGroup.gameObject.DestroyChildren();

            var numStr = num.ToString();
            for (int i = 0; i < numStr.Length; i++)
            {
                int id = numStr[i] - 48;
                
                if(id >= imageNums.Count)
                {
                    throw new Exception($"imageNums count not enough, count : {imageNums.Count},num id : {id}");
                }
                var sprite = imageNums[id];
                var graphGO = new GameObject($"{id}", typeof(Image));
                graphGO.transform.SetParent(hGroup.transform, false);
                graphGO.GetComponent<Image>().sprite = sprite;
            }
        }
    }
}
