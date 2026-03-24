#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Experimental.Rendering;

namespace PowerUtilities
{
    public class TestAttributes : MonoBehaviour
    {
        /**
         EditorGroup and EditorGroup(Item)
         */
        [EditorGroup("Group1", true)]
        public bool group1On;
        [EditorGroup("Group1")]
        public string group1Name = "group1";

        // --------- group2
        [EditorGroup("Group2", true)]
        public bool Group2On;

        [EditorGroup("Group2")]
        public float a;

        /**
         EnumFlags
         */
        [Flags]
        public enum PropEnum {
            //[InspectorName("class/a")]
            a,
            //[InspectorName("class/b")]
            b,
            c };
        [EnumFlags(isFlags = false)]
        [EditorHeader("","EnumFlags")]
        public PropEnum propEnum;

        /**
         DisplayName
         */
        [DisplayName("一个滑块", "slider helps", "Assets/Gizmos/img1.png")]
        [Range(0, 1)]
        [EditorHeader("", "DisplayName")]
        public float propA;



        /**
         TexturePreview
         */
        [TexturePreview]
        [EditorHeader("", "TexturePreview")]
        public Texture tex;
        /**
         LayerIndex
         */
        [EditorHeader("", "LayerIndex")]
        [LayerIndex]
        public int layerId;
        /**
         HelpBox
         */
        [EditorHeader("", "HelpBox")]
        [HelpBox]
        public string helpStr = "help text";
        /**
         SortingLayerIndex
         */
        [EditorHeader("", "SortingLayerIndex")]
        [SortingLayerIndex]
        public int sortingLayerId;


        /**
         ListItemDraw
         */

        [Serializable]
        public class PersonInfo
        {
            public string name;
            public int age;
            public bool isValid;
        }
        [EditorHeader("", "ListItemDraw")]
        [ListItemDraw("名,name,年龄,age,法外狂徒,isValid", "20,50,30,50,100,20")]
        public PersonInfo[] testListItemDraw = new[]{
            new PersonInfo{name="张三1",age=123,isValid = false },
            new PersonInfo{name="张三",age=123,isValid = true },
        };

        /**
         EditorTextFieldWithMenu
         */
        static string[] menuNames => new[] {"a","b","c" };
        [EditorHeader("", "EditorTextFieldWithMenu")]
        [EditorTextFieldWithMenuAttribute(type = typeof(TestAttributes), staticMemberName = "menuNames")]
        public string textInput;

        /**
         EditorDisableGroup
         */
        [EditorHeader("", "EditorDisableGroup")]
        [EditorDisableGroup(targetPropName = "group1On")]
        public string disabledText = "readonly info";

        /**
         EditorToolbar
         */
        [EditorHeader("", "EditorToolbar")]
        [EditorToolbar(onClickCall = "OnButtonClick2",texts = new[] { "button1","button2", "button3" })]
        public bool buttonA;
        public int editorToolbarValue;
        public void OnButtonClick2(int id)
        {
            editorToolbarValue = id;
            Debug.Log("click "+id);
        }
        public void OnButtonClick()
        => Debug.Log("click ");

        /**
         EditorButton
         */
        [EditorButton(onClickCall ="OnButtonClick")]
        public bool buttonB;

        [EditorHeader("", "EditorBorder")]
        [EditorBorder(4)]
        [TextArea]
        public string showBorder;

        [EditorHeader("", "EnumSearchable")]
        [EnumSearchable(typeof(GraphicsFormat))]
        public GraphicsFormat gFormat;
        
        [EditorHeader("", "StringListSearchable")]
        [StringListSearchable(names = "none,tag/tag1,tag/tag2,name/n1,name/n2")]
        public string tagOrName;
#if UNITY_EDITOR
        [StringListSearchable(type = typeof(TagManager), staticMemberName = nameof(TagManager.GetTags))]
        public string tags;
#endif
        [EditorHeader("", "EditorSceneView")]
        [ShowInSceneView(containerType = typeof(Vector3[]))]
        public Vector3[] posArray = new[] { Vector3.zero,Vector3.one};
        public Vector4 testPos;

        [EditorHeader("","Box")]
        [EditorBox("Box", "showBox1,showBox2",isShowFoldout =true,boxType = EditorBoxAttribute.BoxType.HBox)]
        public string showBox1;
        public string showBox2;

        [EditorScrollView(lineCount =10)]
        //[Multiline]
        [EditorIntent(1)]
        public string texts;
    }
}
#endif