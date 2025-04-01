namespace PowerUtilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using UnityEngine;

    /**
     
        [EditorBoxAttribute("Path Tools Buttons", "isSetSpline,isReadSpline,isStampPaths", isShowFoldout = true, boxType = EditorBoxAttribute.BoxType.HBox)]
        [EditorButton(onClickCall = "SetSpline")]
        [Tooltip("set poslist to mainSpline")]
        public bool isSetSpline;

        [EditorButton(onClickCall = "ReadSpline")]
        [HideInInspector] public bool isReadSpline;

        [EditorButton(onClickCall = "StampPaths")]
        [HideInInspector] public bool isStampPaths;
     */
    public class EditorBoxAttribute : PropertyAttribute
    {
        public enum BoxType
        {
            VBox,HBox
        }

        public BoxType boxType;
        
        /// <summary>
        /// property name array,like Name1,Name2
        /// </summary>
        string[] propNames;
        public string[] PropNames => propNames;
        /// <summary>
        /// property width rate, like 0.3,0.5
        /// </summary>
        public float[] propWidthRates;

        /// <summary>
        /// Show a title
        /// </summary>
        public string header;
        /// <summary>
        /// Show Content
        /// </summary>
        public bool isShowFoldout;

        [HideInInspector]
        public bool isFolded=true;



        /// <summary>
        /// Show EditorGUI in Box(hbox,vbox)
        /// </summary>
        /// <param name="header"></param>
        /// <param name="propNamesStr"></param>
        public EditorBoxAttribute(string header, string propNamesStr, string propWidthRatesStr = "")
        {
            this.header = header;
            if (!string.IsNullOrEmpty(propNamesStr))
                propNames = propNamesStr.SplitBy();

            if (!string.IsNullOrEmpty(propWidthRatesStr))
            {
                propWidthRates = propWidthRatesStr.SplitBy().
                    Select(floatItem => Convert.ToSingle(floatItem))
                    .ToArray();
            }
            else
            {
                propWidthRates = ArrayTools.Create(propNames.Length, 1f);
            }
        }
    }
}
