namespace PowerUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    public class ImageNumberFX : MonoBehaviour
    {
        public enum RenderType
        {
            SpriteRenderer,UGUIImage
        }

        [EditorHeader("","0.0.2")]
        [HelpBox]
        public string help = "[0-9] number image for ugui";

        public RenderType renderType;

        public int num = 123;
        public List<Sprite> imageNums = new List<Sprite>();
        [Header("Mat")]
        public Material spriteMat;

        [Header("Sprite Space(SpriteRenderer)")]
        [Tooltip("SpriteRenderer 's Spacing")]
        public float spacing = 0;

        public bool isOverrideSpriteSize;
        [Tooltip("Sprite target pixel size")]
        [EditorDisableGroup(targetPropName = "isOverrideSpriteSize")]
        public Vector2 spriteTargetSize = Vector2.zero;

        [EditorButton(onClickCall = "Sort")]
        public bool isSort;

        public bool isSortWhenEnable;

        [Header("Life")]
        public float lifeTime = 3;

        [Header("Anim")]
        public bool isUseAnim;
        public Vector3 moveDir = Vector3.up;
        public AnimationCurve moveCurve = new AnimationCurve(new Keyframe(0,1),new Keyframe(1,1));

        public AnimationCurve scaleCurve=new AnimationCurve(new Keyframe(0,1),new Keyframe(1,1));
        float startTime;
        Vector3 lastLocalScale;

        // ugui
        HorizontalLayoutGroup hGroup;
        int lastNum;


        // Start is called before the first frame update
        void OnEnable()
        {
            if (isSortWhenEnable)
                Sort();

            startTime = Time.time;
            lastLocalScale = transform.localScale;
            lastNum = num;
        }

        private void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        // Update is called once per frame
        void Update()
        {
            TrySort();
            TryMove();
        }
        void TryMove()
        {
            if (!isUseAnim)
                return;

            var t = (Time.time - startTime);
            var speed = moveCurve.Evaluate(t) * Time.deltaTime;
            transform.localPosition += moveDir * speed;

            var scaleSpeed = scaleCurve.Evaluate(t);
            transform.localScale = lastLocalScale * scaleSpeed;
        }
        private void TrySort()
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



        private void SetupSprite(int id, Sprite sprite, GameObject spriteGO)
        {
            if (renderType == RenderType.SpriteRenderer)
            {
                var sr = spriteGO.GetOrAddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                if(spriteMat)
                    sr.sharedMaterial = spriteMat;
                
                if (isOverrideSpriteSize)
                {
                    sprite.SetSpriteScale(spriteGO.transform, spriteTargetSize);
                }

                var posX = (sprite.bounds.size.x * spriteGO.transform.localScale.x + spacing) * id;
                
                var pos = spriteGO.transform.localPosition;
                pos.x = posX;
                spriteGO.transform.localPosition = pos;
            }
            else
            {
                if (!hGroup)
                    hGroup = gameObject.GetOrAddComponent<HorizontalLayoutGroup>();

                spriteGO.GetOrAddComponent<Image>().sprite = sprite;
            }
        }

    }
}
